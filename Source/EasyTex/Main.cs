using HarmonyLib;
using System;
using System.IO;
using UnityEngine;
using UnityModManagerNet;
using DV.ThingTypes;

namespace EasyTex
{
	public class Main
	{
		static UnityModManager.ModEntry _mod;
		static AssetBundle bundle;
		static GameObject[] patchedPrefabs = { null };
		//static bool slicedCarsInstalled = false;

		static void Load(UnityModManager.ModEntry mod)
		{
			// Setup UMM
			_mod = mod;

			// Sliced passenger cars is obsolete
			/*slicedCarsInstalled = Directory.Exists(Path.Combine(mod.Path, "../SlicedPassengerCars"));
			if (slicedCarsInstalled)
				mod.Logger.Warning("Sliced Passenger Cars mod detected! Patched passenger cars have been disabled.");*/
			
			// Setup the harmony patches
			Harmony harmony = new Harmony(_mod.Info.Id);
			harmony.Patch(
				original: AccessTools.Method(typeof(TrainCar), nameof(TrainCar.GetCarPrefab)),
				postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.GetCarPrefab_Patch)));

			DebugLog("Started");
        }

		static void DebugLog(string txt)
		{
#if DEBUG
			_mod.Logger.Log("[EasyTex] " + txt);
#endif
		}

		public class Patches
		{
			private static string[] carTankLODMap = new string[] {
				"car_tanker_lod/car_tanker_LOD0",
				"car_tanker_lod/car_tanker_LOD1",
				"car_tanker_lod/car_tanker_LOD2",
				"car_tanker_lod/car_tanker_LOD3"
			};

			private static string[] carTankLODMap_Sim = new string[]
			{
				"CarTank/CarTank_LOD0",
				"CarTank/CarTank_LOD1",
				"CarTank/CarTank_LOD2",
				"CarTank/CarTank_LOD3"
			};

			private static string[] cabooseLODMap = new string[] {
				"CarCaboose_exterior/CabooseExterior",
				"CarCaboose_exterior/CabooseExterior_LOD1",
				"CarCaboose_exterior/CabooseExterior_LOD2",
				"CarCaboose_exterior/Caboose_LOD3"
			};

			private static string[] flatCarLODMap = new string[] {
				"car_flatcar_lod/flatcar",
				"car_flatcar_lod/car_flatcar_LOD1",
				"car_flatcar_lod/car_flatcar_LOD2",
				"car_flatcar_lod/car_flatcar_LOD3"
			};

			private static string[] boxCarLODMap = new string[] {
				"car_boxcar_lod/car_boxcar",
				"car_boxcar_lod/car_boxcar_LOD1",
				"car_boxcar_lod/car_boxcar_LOD2",
				"car_boxcar_lod/car_boxcar_LOD3"
			};

			private static string[] boxCarLODMap_Sim = new string[] {
				"CarBoxcar/CarBoxcar_LOD0",
				"CarBoxcar/CarBoxcar_LOD1",
				"CarBoxcar/CarBoxcar_LOD2",
				"CarBoxcar/CarBoxcar_LOD3"
			};

			private static string[] passengerLODMap = new string[]
			{
                "car_passenger_lod/car_passenger_LOD0",
                "car_passenger_lod/car_passenger_LOD1",
				"car_passenger_lod/car_passenger_LOD2",
				"car_passenger_lod/car_passenger_LOD3"
			};

            private static string[] passengerLODMap_Sim = new string[]
            {
                "CarPassenger/CarPassenger_LOD0",
                "CarPassenger/CarPassenger_LOD1",
                "CarPassenger/CarPassenger_LOD2",
                "CarPassenger/CarPassenger_LOD3"
            };

            private static string[] patchedPrefabList = new string[]
			{
				"Patched_CarTanker",
				"Patched_CarCaboose",
				"Patched_FlatCar",
				"Patched_BoxCar",
				"Patched_PassengerCar"
			};

			// Return a LOD mapping based on the supplied carType
			private static string[] GetLODs(TrainCarType carType)
			{
				switch (carType)
				{
					case TrainCarType.TankBlack:
					case TrainCarType.TankBlue:
					case TrainCarType.TankChrome:
					case TrainCarType.TankOrange:
					case TrainCarType.TankWhite:
					case TrainCarType.TankYellow:
						DebugLog("Getting Tank Car LODs");
						return carTankLODMap;

					case TrainCarType.CabooseRed:
						DebugLog("Getting Caboose LODs");
						return cabooseLODMap;

					// Disabled, but I'm not willing to remove it completely
					//case TrainCarType.FlatbedEmpty:
					//	return flatCarLODMap;

					case TrainCarType.BoxcarBrown:
					case TrainCarType.BoxcarGreen:
					case TrainCarType.BoxcarPink:
					case TrainCarType.BoxcarRed:
						DebugLog("Getting Box Car LODs");
						return boxCarLODMap;

					// Reworked, maybe works now?
					case TrainCarType.PassengerBlue:
					case TrainCarType.PassengerGreen:
					case TrainCarType.PassengerRed:
						DebugLog("Getting Passenger Car LODs");
                        return passengerLODMap;

					default:
						return null;
				}
			}

			// Return a LOD mapping based on the supplied carType
			private static string[] GetLODs_Sim(TrainCarType carType)
			{
				switch (carType)
				{
					case TrainCarType.TankBlack:
					case TrainCarType.TankBlue:
					case TrainCarType.TankChrome:
					case TrainCarType.TankOrange:
					case TrainCarType.TankWhite:
					case TrainCarType.TankYellow:
                        DebugLog("Sim - Getting Tank Car LODs");
                        return carTankLODMap_Sim;

					case TrainCarType.CabooseRed:
                        DebugLog("Sim - Getting Caboose LODs");
                        return cabooseLODMap;

					// Disabled, but I'm not willing to remove it completely
					//case TrainCarType.FlatbedEmpty:
					//	return flatCarLODMap;

					case TrainCarType.BoxcarBrown:
					case TrainCarType.BoxcarGreen:
					case TrainCarType.BoxcarPink:
					case TrainCarType.BoxcarRed:
                        DebugLog("Sim - Getting Box Car LODs");
                        return boxCarLODMap_Sim;

					case TrainCarType.PassengerBlue:
					case TrainCarType.PassengerGreen:
					case TrainCarType.PassengerRed:
                        DebugLog("Sim - Getting Passenger Car LODs");
                        return passengerLODMap_Sim;

					default:
						return null;
				}
			}

			// Load/reload the prefabs from the asset bundle
			static void LoadPrefabs()
			{
				// Load the patched prefab bundle (if not already)
				if (bundle == null)
					bundle = AssetBundle.LoadFromFile(Path.Combine(_mod.Path, "patched.assets"));

				// Load a bundle with all the patched prefabs contained within				
				if (patchedPrefabs[0] == null)
				{
					patchedPrefabs = new GameObject[patchedPrefabList.Length];
					for (int i = 0; i < patchedPrefabList.Length; i++)
					{
						patchedPrefabs[i] = (GameObject)bundle.LoadAsset(patchedPrefabList[i]);
					}
				}

				DebugLog("Loaded " + patchedPrefabList.Length + "Patches");

				foreach(GameObject go in patchedPrefabs)
				{
					DebugLog(go.name);
				}
			}

			// Return a patchec prefab based on the specified carType
			static GameObject GetPatchedPrefab(TrainCarType carType)
			{
				if (patchedPrefabs[0] == null) LoadPrefabs();

				DebugLog("Patching a " + carType.ToString());

                switch (carType)
				{
					case TrainCarType.TankBlack:
					case TrainCarType.TankBlue:
					case TrainCarType.TankChrome:
					case TrainCarType.TankOrange:
					case TrainCarType.TankWhite:
					case TrainCarType.TankYellow:
						return patchedPrefabs[0];

					case TrainCarType.CabooseRed:
						return patchedPrefabs[1];

					// Disabled, but I'm not willing to remove it completely
					//case TrainCarType.FlatbedEmpty:
					//	return patchedPrefabs[2];

					case TrainCarType.BoxcarBrown:
					case TrainCarType.BoxcarGreen:
					case TrainCarType.BoxcarPink:
					case TrainCarType.BoxcarRed:
						return patchedPrefabs[3];

					case TrainCarType.PassengerBlue:
					case TrainCarType.PassengerGreen:
					case TrainCarType.PassengerRed:
						return patchedPrefabs[4];

					default:
						return null;
				}
			}

			public static void GetCarPrefab_Patch(ref GameObject __result)
			{
				try
				{
					// Return if result is invalid
					if (__result == null)
						return;

					// Get the traincar component
					TrainCar tc = __result.GetComponent<TrainCar>();

					/*if ((
							tc.carType == TrainCarType.PassengerBlue ||
							tc.carType == TrainCarType.PassengerGreen ||
							tc.carType == TrainCarType.PassengerRed
						) &&
						slicedCarsInstalled)
						return;*/

					// Get the LOD tree for the carType, return on invalid carType
					string[] lods = GetLODs(tc.carType);
					string[] lods_sim = GetLODs_Sim(tc.carType);
					if (lods == null || lods_sim == null)
					{ 
						DebugLog("LODs null return");
						return;
					}

					// Patch it!
					// Go through each LOD and copy the UVs from the patched meshes
					// to the OG meshes
					//foreach (string lod, in lods)					
					for(int i = 0; i < lods.Length; i++)
					{						
						Mesh m = GetPatchedPrefab(tc.carType).transform.Find(lods[i]).GetComponent<MeshFilter>().mesh;
			
						m.RecalculateBounds();

						m.RecalculateTangents();
                        m.Optimize();
						__result.transform.Find(lods_sim[i]).GetComponent<MeshFilter>().mesh = m;						
					}
					_mod.Logger.Log("Successfully patched mesh for " + tc.name);
					// LittleLad: Nice AND easy!

				} catch(Exception e)
				{
					_mod.Logger.LogException(e);
				}
			}
		}
	}
}