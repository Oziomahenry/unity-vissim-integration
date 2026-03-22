import win32com.client
import socket
import json
import time
import select

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
                ("No","VehType","CoordFrontX","CoordFrontY","Speed"))
            return [{
                "id": int(v[0]),
                "type": int(v[1]),
                "x": float(v[2]),
                "y": float(v[3]),
                "speed": float(v[4])
            } for v in data]
        except Exception as e:
            print(f"[Warning] {self.name} vehicles: {e}")
            return []

    def get_pedestrians(self):
        try:
            data = self.vissim.Net.Pedestrians.GetMultipleAttributes(
                ("No","PedType","CoordFrontX","CoordFrontY","Speed"))
            return [{
                "id": int(p[0]),
                "type": int(p[1]),
                "x": float(p[2]),
                "y": float(p[3]),
                "speed": float(p[4])
            } for p in data]
        except Exception as e:
            print(f"[Warning] {self.name} pedestrians: {e}")
            return []

    # ----------------------------
    # BUS INPUT (ALL SIMS)
    # ----------------------------
    def set_bus_flow(self, veh_input_id, volume):
        try:
            veh_input = self.vissim.Net.VehicleInputs.ItemByKey(veh_input_id)
            time_intervals = veh_input.TimeIntVehVols

            for i in range(1, time_intervals.Count + 1):
                tiv = time_intervals.ItemByKey(i)
                tiv.SetAttValue("Volume", volume)

        except Exception as e:
            print(f"[Error] {self.name} set_bus_flow: {e}")

    # ----------------------------
    # VEHICLE SPEED (ACTIVE SIM)
    # ----------------------------
    def set_vehicle_speed(self, vehicle_id, speed):
        try:
            v = self.vissim.Net.Vehicles.ItemByKey(vehicle_id)
            v.SetAttValue("Speed", speed)
        except Exception as e:
            print(f"[Error] {self.name} set_vehicle_speed: {e}")


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
        if 0 <= idx < 3:
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
     r"C:\Users\User\Downloads\vissim_withbstop.inpx",
     r"C:\Users\User\Downloads\vissim_withbstop.inpx",
     r"C:\Users\User\Downloads\vissim_withbstop.inpx"
]

manager = SimulationManager(paths)

# ------------------------------------------------------------
# UDP SERVER
# ------------------------------------------------------------

HOST = "127.0.0.1"
PORT = 1234

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((HOST, PORT))
sock.setblocking(False)

unity_addr = None
seq = 0

print("UDP VISSIM server running...")

# ------------------------------------------------------------
# MAIN LOOP
# ------------------------------------------------------------

while True:

    # ----------------------------
    # RECEIVE COMMANDS
    # ----------------------------
    try:
        ready = select.select([sock], [], [], 0)
        if ready[0]:
            data, addr = sock.recvfrom(4096)

            if unity_addr is None:
                unity_addr = addr
                print("Unity connected:", unity_addr)

            cmd = json.loads(data.decode("utf-8"))

            # SWITCH SIMULATION
            if cmd.get("type") == "select_sim":
                manager.set_active(cmd.get("index", 0))

            # BUS VOLUME (ALL SIMS)
            elif cmd.get("type") == "bus_input":
                manager.apply_bus_to_all(
                    cmd.get("id"),
                    cmd.get("volume")
                )

            # VEHICLE SPEED (ACTIVE SIM)
            elif cmd.get("type") == "set_speed":
                manager.set_speed_active(
                    cmd.get("id"),
                    cmd.get("speed")
                )

    except Exception:
        pass

    # ----------------------------
    # STEP ACTIVE SIMULATION
    # ----------------------------
    manager.step_active()
    seq += 1

    # ----------------------------
    # GET DATA
    # ----------------------------
    vehicles, pedestrians = manager.get_active_data()

    # ----------------------------
    # SEND TO UNITY
    # ----------------------------
    if unity_addr:
        msg = json.dumps({
            "type": "state",
            "seq": seq,
            "vehicles": vehicles,
            "pedestrians": pedestrians
        })

        try:
            sock.sendto(msg.encode("utf-8"), unity_addr)
        except Exception as e:
            print("[Warning] send failed:", e)

    time.sleep(0.01)