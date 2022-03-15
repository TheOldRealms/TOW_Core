using SandBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Utilities
{
    public class TORWorldMapScript : ScriptComponentBehavior
	{
		private static string SettlementsXmlPath
		{
			get
			{
				return ModuleHelper.GetModuleFullPath("TOW_Core") + "ModuleData/tow_settlements.xml";
			}
		}

		private static string SettlementsDistanceCacheFilePath
		{
			get
			{
				return ModuleHelper.GetModuleFullPath("TOW_Core") + "ModuleData/settlements_distance_cache.bin";
			}
		}

		protected override void OnEditorVariableChanged(string variableName)
		{
			base.OnEditorVariableChanged(variableName);
			if (variableName == "SavePositions")
			{
				this.SaveSettlementPositions();
			}
			if (variableName == "ComputeAndSaveSettlementDistanceCache")
			{
				this.SaveSettlementDistanceCache();
			}
			if (variableName == "CheckPositions")
			{
				this.CheckSettlementPositions();
			}
		}

		protected override void OnSceneSave(string saveFolder)
		{
			base.OnSceneSave(saveFolder);
			this.SaveSettlementPositions();
			this.SaveSettlementDistanceCache();
		}

		private void CheckSettlementPositions()
		{
			XmlDocument xmlDocument = this.LoadXmlFile(TORWorldMapScript.SettlementsXmlPath);
			base.GameEntity.RemoveAllChildren();
			foreach (object obj in xmlDocument.DocumentElement.SelectNodes("Settlement"))
			{
				string value = ((XmlNode)obj).Attributes["id"].Value;
				GameEntity campaignEntityWithName = base.Scene.GetCampaignEntityWithName(value);
				Vec3 origin = campaignEntityWithName.GetGlobalFrame().origin;
				Vec3 vec = default(Vec3);
				List<GameEntity> list = new List<GameEntity>();
				campaignEntityWithName.GetChildrenRecursive(ref list);
				bool flag = false;
				foreach (GameEntity gameEntity in list)
				{
					if (gameEntity.HasTag("main_map_city_gate"))
					{
						vec = gameEntity.GetGlobalFrame().origin;
						flag = true;
						break;
					}
				}
				Vec3 pos = origin;
				if (flag)
				{
					pos = vec;
				}
				PathFaceRecord pathFaceRecord = new PathFaceRecord(-1, -1, -1);
				base.GameEntity.Scene.GetNavMeshFaceIndex(ref pathFaceRecord, pos.AsVec2, true, false);
				int num = 0;
				if (pathFaceRecord.IsValid())
				{
					num = pathFaceRecord.FaceGroupIndex;
				}
				if (num == 0 || num == 7 || num == 8 || num == 10 || num == 11 || num == 13 || num == 14)
				{
					MBEditor.ZoomToPosition(pos);
					break;
				}
			}
		}

		protected override void OnInit()
		{
			try
			{
				Debug.Print("SettlementsDistanceCacheFilePath: " + TORWorldMapScript.SettlementsDistanceCacheFilePath, 0, Debug.DebugColor.White, 17592186044416UL);
				System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(File.Open(TORWorldMapScript.SettlementsDistanceCacheFilePath, FileMode.Open, FileAccess.Read));
				if (Campaign.Current.Models.MapDistanceModel is DefaultMapDistanceModel)
				{
					((DefaultMapDistanceModel)Campaign.Current.Models.MapDistanceModel).LoadCacheFromFile(binaryReader);
				}
				binaryReader.Close();
			}
			catch
			{
				Debug.Print("SettlementsDistanceCacheFilePath could not be read!. Campaign performance will be affected very badly.", 0, Debug.DebugColor.White, 17592186044416UL);
			}
		}

		private List<TORWorldMapScript.TowSettlementRecord> LoadSettlementData(XmlDocument settlementDocument)
		{
			List<TORWorldMapScript.TowSettlementRecord> list = new List<TORWorldMapScript.TowSettlementRecord>();
			base.GameEntity.RemoveAllChildren();
			foreach (object obj in settlementDocument.DocumentElement.SelectNodes("Settlement"))
			{
				XmlNode xmlNode = (XmlNode)obj;
				string value = xmlNode.Attributes["name"].Value;
				string value2 = xmlNode.Attributes["id"].Value;
				GameEntity campaignEntityWithName = base.Scene.GetCampaignEntityWithName(value2);
				Vec2 asVec = campaignEntityWithName.GetGlobalFrame().origin.AsVec2;
				Vec2 vec = default(Vec2);
				List<GameEntity> list2 = new List<GameEntity>();
				campaignEntityWithName.GetChildrenRecursive(ref list2);
				bool flag = false;
				foreach (GameEntity gameEntity in list2)
				{
					if (gameEntity.HasTag("main_map_city_gate"))
					{
						vec = gameEntity.GetGlobalFrame().origin.AsVec2;
						flag = true;
					}
				}
				list.Add(new TORWorldMapScript.TowSettlementRecord(value, value2, asVec, flag ? vec : asVec, xmlNode, flag));
			}
			return list;
		}

		private void SaveSettlementPositions()
		{
			XmlDocument xmlDocument = this.LoadXmlFile(TORWorldMapScript.SettlementsXmlPath);
			foreach (TORWorldMapScript.TowSettlementRecord settlementRecord in this.LoadSettlementData(xmlDocument))
			{
				if (settlementRecord.Node.Attributes["posX"] == null)
				{
					XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("posX");
					settlementRecord.Node.Attributes.Append(xmlAttribute);
				}
				settlementRecord.Node.Attributes["posX"].Value = settlementRecord.Position.X.ToString();
				if (settlementRecord.Node.Attributes["posY"] == null)
				{
					XmlAttribute xmlAttribute2 = xmlDocument.CreateAttribute("posY");
					settlementRecord.Node.Attributes.Append(xmlAttribute2);
				}
				settlementRecord.Node.Attributes["posY"].Value = settlementRecord.Position.Y.ToString();
				if (settlementRecord.HasGate)
				{
					if (settlementRecord.Node.Attributes["gate_posX"] == null)
					{
						XmlAttribute xmlAttribute3 = xmlDocument.CreateAttribute("gate_posX");
						settlementRecord.Node.Attributes.Append(xmlAttribute3);
					}
					settlementRecord.Node.Attributes["gate_posX"].Value = settlementRecord.GatePosition.X.ToString();
					if (settlementRecord.Node.Attributes["gate_posY"] == null)
					{
						XmlAttribute xmlAttribute4 = xmlDocument.CreateAttribute("gate_posY");
						settlementRecord.Node.Attributes.Append(xmlAttribute4);
					}
					settlementRecord.Node.Attributes["gate_posY"].Value = settlementRecord.GatePosition.Y.ToString();
				}
			}
			xmlDocument.Save(TORWorldMapScript.SettlementsXmlPath);
		}

		private void SaveSettlementDistanceCache()
		{
			System.IO.BinaryWriter binaryWriter = null;
			try
			{
				XmlDocument settlementDocument = this.LoadXmlFile(TORWorldMapScript.SettlementsXmlPath);
				List<TORWorldMapScript.TowSettlementRecord> list = this.LoadSettlementData(settlementDocument);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.Mountain), false);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.Lake), false);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.Water), false);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.River), false);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.Canyon), false);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.RuralArea), false);
				binaryWriter = new System.IO.BinaryWriter(File.Open(TORWorldMapScript.SettlementsDistanceCacheFilePath, FileMode.Create));
				binaryWriter.Write(list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					binaryWriter.Write(list[i].SettlementId);
					Vec2 gatePosition = list[i].GatePosition;
					PathFaceRecord pathFaceRecord = new PathFaceRecord(-1, -1, -1);
					base.Scene.GetNavMeshFaceIndex(ref pathFaceRecord, gatePosition, false, false);
					for (int j = i + 1; j < list.Count; j++)
					{
						binaryWriter.Write(list[j].SettlementId);
						Vec2 gatePosition2 = list[j].GatePosition;
						PathFaceRecord pathFaceRecord2 = new PathFaceRecord(-1, -1, -1);
						base.Scene.GetNavMeshFaceIndex(ref pathFaceRecord2, gatePosition2, false, false);
						float num;
						base.Scene.GetPathDistanceBetweenAIFaces(pathFaceRecord.FaceIndex, pathFaceRecord2.FaceIndex, gatePosition, gatePosition2, 0.1f, float.MaxValue, out num);
						binaryWriter.Write(num);
					}
				}
			}
			catch
			{
			}
			finally
			{
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.Mountain), true);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.Lake), true);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.Water), true);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.River), true);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.Canyon), true);
				base.Scene.SetAbilityOfFacesWithId(MapScene.GetNavigationMeshIndexOfTerrainType(TerrainType.RuralArea), true);
				if (binaryWriter != null)
				{
					binaryWriter.Close();
				}
			}
		}

		private XmlDocument LoadXmlFile(string path)
		{
			Debug.Print("opening " + path, 0, Debug.DebugColor.White, 17592186044416UL);
			XmlDocument xmlDocument = new XmlDocument();
			StreamReader streamReader = new StreamReader(path);
			string text = streamReader.ReadToEnd();
			xmlDocument.LoadXml(text);
			streamReader.Close();
			return xmlDocument;
		}

		protected override bool IsOnlyVisual()
		{
			return true;
		}

		public SimpleButton CheckPositions;

		public SimpleButton SavePositions;

		public SimpleButton ComputeAndSaveSettlementDistanceCache;

		private struct TowSettlementRecord
		{
			public TowSettlementRecord(string settlementName, string settlementId, Vec2 position, Vec2 gatePosition, XmlNode node, bool hasGate)
			{
				this.SettlementName = settlementName;
				this.SettlementId = settlementId;
				this.Position = position;
				this.GatePosition = gatePosition;
				this.Node = node;
				this.HasGate = hasGate;
			}

			public readonly string SettlementName;

			public readonly string SettlementId;

			public readonly XmlNode Node;

			public readonly Vec2 Position;

			public readonly Vec2 GatePosition;

			public readonly bool HasGate;
		}
	}
}
