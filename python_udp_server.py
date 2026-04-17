import win32com.client
import socket
import json
import time
import select

# ------------------------------------------------------------
# VISSIM SIMULATION CLASS
# ------------------------------------------------------------

class VissimSimulation:
    def __init__(self, vissim):
        self.vissim = vissim

    # ---------------- STEP ----------------
    def step(self):
        try:
            self.vissim.Simulation.RunSingleStep()
        except Exception as e:
            print("[STEP ERROR]", e)

    # ---------------- GET DATA ----------------
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
            print("[Warning] vehicles:", e)
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
            print("[Warning] pedestrians:", e)
            return []

    # ---------------- AUTO VEHICLE SPEED ----------------
    def set_vehicle_speed_auto(self, speed):
        try:
            vehicles = self.vissim.Net.Vehicles.GetAll()

            if not vehicles:
                print("[WARNING] No vehicles available to control")
                return

            v = vehicles[0]  # 🔥 auto-pick first available vehicle
            vid = v.AttValue("No")

            v.SetAttValue("Speed", speed)

            print(f"[AUTO SPEED] Vehicle {vid} → {v.AttValue('Speed')}")

        except Exception as e:
            print("[Error] set_vehicle_speed_auto:", e)

    # ---------------- AUTO BUS INPUT DETECTION ----------------
    def get_first_vehicle_input_id(self):
        try:
            inputs = self.vissim.Net.VehicleInputs.GetAll()

            if not inputs:
                print("[WARNING] No vehicle inputs found")
                return None

            vid = inputs[0].AttValue("No")
            return vid

        except Exception as e:
            print("[Error] get_first_vehicle_input_id:", e)
            return None

    # ---------------- AUTO BUS FLOW ----------------
    def set_bus_flow_auto(self, volume):
        try:
            veh_input_id = self.get_first_vehicle_input_id()

            if veh_input_id is None:
                return

            print(f"[AUTO BUS] Using VehicleInput {veh_input_id} → volume={volume}")

            veh_input = self.vissim.Net.VehicleInputs.ItemByKey(veh_input_id)
            time_intervals = veh_input.TimeIntVehVols

            for i in range(1, time_intervals.Count + 1):
                tiv = time_intervals.ItemByKey(i)
                tiv.SetAttValue("Volume", volume)

            print("[VISSIM CONFIRM] Bus volume updated")

        except Exception as e:
            print("[Error] set_bus_flow_auto:", e)

    # ---------------- DEBUG AVAILABLE IDS ----------------
    def debug_ids(self):
        try:
            vehicles = self.vissim.Net.Vehicles.GetAll()
            inputs = self.vissim.Net.VehicleInputs.GetAll()

            print("\n--- DEBUG IDS ---")

            print("Vehicles:")
            for v in vehicles[:5]:
                print("  ID:", v.AttValue("No"))

            print("VehicleInputs:")
            for inp in inputs:
                print("  ID:", inp.AttValue("No"))

            print("------------------\n")

        except Exception as e:
            print("[Error] debug_ids:", e)


# ------------------------------------------------------------
# SIMULATION MANAGER (SINGLE INSTANCE)
# ------------------------------------------------------------

class SimulationManager:
    def __init__(self, paths):
        self.paths = paths
        self.vissim = win32com.client.Dispatch("Vissim.Vissim")
        self.sim = VissimSimulation(self.vissim)

        self.active_index = -1
        self.load_simulation(0)

    def load_simulation(self, idx):
        try:
            print(f"[Manager] Loading scenario {idx}")

            self.vissim.LoadNet(self.paths[idx])
            self.vissim.Simulation.SetAttValue("SimRes", 1)
            self.vissim.Graphics.CurrentNetworkWindow.SetAttValue("QuickMode", 1)

            self.active_index = idx

            # 🔥 DEBUG IDS AFTER LOAD
            self.sim.debug_ids()

        except Exception as e:
            print("[ERROR loading scenario]", e)

    def set_active(self, idx):
        if idx != self.active_index:
            self.load_simulation(idx)

    def step(self):
        self.sim.step()

    def get_data(self):
        return self.sim.get_vehicles(), self.sim.get_pedestrians()

    def set_speed(self, speed):
        self.sim.set_vehicle_speed_auto(speed)

    def set_bus_volume(self, volume):
        self.sim.set_bus_flow_auto(volume)


# ------------------------------------------------------------
# LOAD NETWORKS (FIX YOUR PATHS HERE!)
# ------------------------------------------------------------

paths = [
    r"C:\Users\User\unity-vissim integration\vissim_withbstop_final_version.inpx",
    r"C:\Users\User\unity-vissim integration\vissim_withbstop_ variant 1.1 - Copy.inpx",
    r"C:\Users\User\unity-vissim integration\var 3 - Copy.inpx"
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

print("🚀 UDP server running...")

# ------------------------------------------------------------
# MAIN LOOP
# ------------------------------------------------------------

while True:

    # -------- RECEIVE --------
    try:
        ready = select.select([sock], [], [], 0)
        if ready[0]:
            data, addr = sock.recvfrom(4096)

            if unity_addr is None:
                unity_addr = addr
                print("🎮 Unity connected:", unity_addr)

            cmd = json.loads(data.decode("utf-8"))

            if cmd.get("type") == "select_sim":
                manager.set_active(cmd.get("index", 0))

            elif cmd.get("type") == "set_speed":
                manager.set_speed(cmd.get("speed"))

            elif cmd.get("type") == "bus_input":
                manager.set_bus_volume(cmd.get("volume"))

    except Exception:
        pass

    # -------- STEP --------
    manager.step()
    seq += 1

    # -------- DATA --------
    vehicles, pedestrians = manager.get_data()

    # -------- DEBUG --------
    if seq % 100 == 0:
        print(f"[LIVE] sim={manager.active_index} | vehicles={len(vehicles)}")

    # -------- SEND --------
    if unity_addr:
        msg = json.dumps({
            "type": "state",
            "seq": seq,
            "sim": manager.active_index,
            "vehicles": vehicles,
            "pedestrians": pedestrians
        })

        try:
            sock.sendto(msg.encode("utf-8"), unity_addr)
        except Exception as e:
            print("[Warning] send failed:", e)

    time.sleep(0.01)