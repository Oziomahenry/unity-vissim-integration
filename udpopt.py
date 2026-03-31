import win32com.client
import socket
import time
import orjson

# ------------------------------------------------------------
# CONFIG
# ------------------------------------------------------------
SEND_INTERVAL = 0.05   # 20 Hz network send
DATA_INTERVAL = 0.05   # 20 Hz data fetch
SLEEP_TIME = 0.001     # CPU friendly

HOST = "127.0.0.1"
PORT = 1234


# ------------------------------------------------------------
# VISSIM SIMULATION CLASS
# ------------------------------------------------------------

class VissimSimulation:
    def __init__(self, filepath, name):
        self.name = name
        self.vissim = win32com.client.Dispatch("Vissim.Vissim")
        self.vissim.LoadNet(filepath)

        self.vissim.Simulation.SetAttValue("SimRes", 1)
        self.vissim.Graphics.CurrentNetworkWindow.SetAttValue("QuickMode", 1)

    def step(self):
        self.vissim.Simulation.RunSingleStep()

    def get_vehicles(self):
        try:
            data = self.vissim.Net.Vehicles.GetMultipleAttributes(
                ("No", "VehType", "CoordFrontX", "CoordFrontY", "Speed")
            )
            return [[int(v[0]), int(v[1]), float(v[2]), float(v[3]), float(v[4])] for v in data]
        except Exception as e:
            print(f"[Warning] {self.name} vehicles:", e)
            return []

    def get_pedestrians(self):
        try:
            data = self.vissim.Net.Pedestrians.GetMultipleAttributes(
                ("No", "PedType", "CoordFrontX", "CoordFrontY", "Speed")
            )
            return [[int(p[0]), int(p[1]), float(p[2]), float(p[3]), float(p[4])] for p in data]
        except Exception as e:
            print(f"[Warning] {self.name} pedestrians:", e)
            return []

    def set_bus_flow(self, veh_input_id, volume):
        try:
            veh_input = self.vissim.Net.VehicleInputs.ItemByKey(veh_input_id)
            time_intervals = veh_input.TimeIntVehVols

            for i in range(1, time_intervals.Count + 1):
                tiv = time_intervals.ItemByKey(i)
                tiv.SetAttValue("Volume", volume)

        except Exception as e:
            print(f"[Error] {self.name} set_bus_flow:", e)

    def set_vehicle_speed(self, vehicle_id, speed):
        try:
            v = self.vissim.Net.Vehicles.ItemByKey(vehicle_id)
            v.SetAttValue("Speed", speed)
        except Exception as e:
            print(f"[Error] {self.name} set_vehicle_speed:", e)


# ------------------------------------------------------------
# SIMULATION MANAGER
# ------------------------------------------------------------

class SimulationManager:
    def __init__(self, paths):
        self.simulations = [
            VissimSimulation(paths[0], "sc0"),
            VissimSimulation(paths[1], "sc1"),
            VissimSimulation(paths[2], "sc2"),
        ]
        self.active_index = 0

    def set_active(self, idx):
        if 0 <= idx < len(self.simulations):
            self.active_index = idx
            print(f"[Manager] Active simulation: sc{idx}")

    def step_active(self):
        self.simulations[self.active_index].step()

    def get_active_data(self):
        sim = self.simulations[self.active_index]
        return sim.get_vehicles(), sim.get_pedestrians()

    def apply_bus_to_all(self, veh_input_id, volume):
        for sim in self.simulations:
            sim.set_bus_flow(veh_input_id, volume)

    def set_speed_active(self, vehicle_id, speed):
        self.simulations[self.active_index].set_vehicle_speed(vehicle_id, speed)


# ------------------------------------------------------------
# LOAD NETWORKS
# ------------------------------------------------------------


paths = [    
    r"C:\Users\User\unity-vissim integration\var 3 - Copy.inpx",
    r"C:\Users\User\unity-vissim integration\vissim_withbstop_ variant 1.1 - Copy.inpx",
    r"C:\Users\User\unity-vissim integration\vissim_withbstop_final_version.inpx"
  
    ]

manager = SimulationManager(paths)


# ------------------------------------------------------------
# UDP SERVER SETUP
# ------------------------------------------------------------

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((HOST, PORT))
sock.setblocking(False)

# Increase buffers (important for performance)
sock.setsockopt(socket.SOL_SOCKET, socket.SO_SNDBUF, 65536)
sock.setsockopt(socket.SOL_SOCKET, socket.SO_RCVBUF, 65536)

unity_addr = None
seq = 0

print("🚀 Optimized UDP VISSIM server running...")


# ------------------------------------------------------------
# MAIN LOOP
# ------------------------------------------------------------

last_send = 0
last_data = 0

vehicles = []
pedestrians = []

while True:
    now = time.time()

    # ----------------------------
    # RECEIVE COMMANDS (NON-BLOCKING)
    # ----------------------------
    try:
        data, addr = sock.recvfrom(4096)

        if unity_addr is None:
            unity_addr = addr
            print("Unity connected:", unity_addr)

        cmd = orjson.loads(data)

        cmd_type = cmd.get("type")

        if cmd_type == "select_sim":
            manager.set_active(cmd.get("index", 0))

        elif cmd_type == "bus_input":
            manager.apply_bus_to_all(
                cmd.get("id"),
                cmd.get("volume")
            )

        elif cmd_type == "set_speed":
            manager.set_speed_active(
                cmd.get("id"),
                cmd.get("speed")
            )

    except BlockingIOError:
        pass
    except Exception as e:
        print("[Warning] recv error:", e)

    # ----------------------------
    # STEP SIMULATION
    # ----------------------------
    manager.step_active()
    seq += 1

    # ----------------------------
    # GET DATA (THROTTLED)
    # ----------------------------
    if now - last_data >= DATA_INTERVAL:
        last_data = now
        vehicles, pedestrians = manager.get_active_data()

    # ----------------------------
    # SEND DATA (THROTTLED)
    # ----------------------------
    if unity_addr and (now - last_send) >= SEND_INTERVAL:
        last_send = now

        try:
            msg = orjson.dumps({
                "t": "s",                     # type
                "q": seq,                    # sequence
                "a": manager.active_index,  # active scenario
                "v": vehicles,              # vehicles
                "p": pedestrians            # pedestrians
            })

            sock.sendto(msg, unity_addr)

        except Exception as e:
            print("[Warning] send failed:", e)

    # ----------------------------
    # SMALL SLEEP (CPU SAFE)
    # ----------------------------
    time.sleep(SLEEP_TIME)