using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineRuntime38;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SpineViewer.Spine.Implementations.SkeletonConverter
{
    [SkeletonConverterImplementation(Version.V38)]
    class SkeletonConverter38 : SpineViewer.Spine.SkeletonConverter
    {
        private SkeletonReader reader = null;
        private bool nonessential = false;
        private JsonObject root = null;

        protected override JsonObject ReadBinary(string binPath)
        {
            using var input = File.OpenRead(binPath);
            reader = new(input);

            var result = root = [];
            root["skeleton"] = ReadSkeleton();
            ReadStrings();
            root["bones"] = ReadBones();
            root["slots"] = ReadSlots();
            root["ik"] = ReadIK();
            root["transform"] = ReadTransform();
            root["path"] = ReadPath();
            root["skins"] = ReadSkins();
            root["events"] = ReadEvents();
            root["aimations"] = ReadAnimations();

            reader = null;
            nonessential = false;
            root = null;

            return result;
        }

        private JsonObject ReadSkeleton()
        {
            JsonObject skeleton = [];
            skeleton["hash"] = reader.ReadString();
            skeleton["spine"] = reader.ReadString();
            skeleton["x"] = reader.ReadFloat();
            skeleton["y"] = reader.ReadFloat();
            skeleton["width"] = reader.ReadFloat();
            skeleton["height"] = reader.ReadFloat();
            nonessential = reader.ReadBoolean();
            if (nonessential)
            {
                skeleton["fps"] = reader.ReadFloat();
                skeleton["images"] = reader.ReadString();
                skeleton["audio"] = reader.ReadString();
            }
            return skeleton;
        }

        private void ReadStrings()
        {
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
                reader.StringTable.Add(reader.ReadString());
        }

        private JsonArray ReadBones()
        {
            JsonArray bones = []; 
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                if (i != 0) data["parent"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["rotation"] = reader.ReadFloat();
                data["x"] = reader.ReadFloat();
                data["y"] = reader.ReadFloat();
                data["scaleX"] = reader.ReadFloat();
                data["scaleY"] = reader.ReadFloat();
                data["shearX"] = reader.ReadFloat();
                data["shearY"] = reader.ReadFloat();
                data["length"] = reader.ReadFloat();
                data["transform"] = SkeletonBinary.TransformModeValues[reader.ReadVarInt()].ToString();
                data["skin"] = reader.ReadBoolean();
                if (nonessential) reader.ReadInt();
                bones.Add(data);
            }
            return bones;
        }

        private JsonArray ReadSlots()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray slots = []; 
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["bone"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["color"] = reader.ReadInt().ToString("x8"); // 0xrrggbbaa -> rrggbbaa
                int dark = reader.ReadInt();
                if (dark != -1) data["dark"] = dark.ToString("x6"); // 0x00rrggbb -> rrggbb
                data["attachment"] = reader.ReadStringRef();
                data["blend"] = ((BlendMode)reader.ReadVarInt()).ToString();
                slots.Add(data);
            }
            return slots;
        }

        private JsonArray ReadIK()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray ik = []; 
            int count = reader.ReadVarInt();
            for (int i = 0, bonesCount; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                JsonArray bonesArray = []; bonesCount = reader.ReadVarInt();
                for (int j = 0; j < bonesCount; j++) bonesArray.Add(bones[reader.ReadVarInt()]["name"].GetValue<string>());
                data["bones"] = bonesArray;
                data["target"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["mix"] = reader.ReadFloat();
                data["softness"] = reader.ReadFloat();
                data["bendPositive"] = reader.ReadSByte() > 0;
                data["compress"] = reader.ReadBoolean();
                data["stretch"] = reader.ReadBoolean();
                data["uniform"] = reader.ReadBoolean();
                ik.Add(data);
            }
            return ik;
        }

        private JsonArray ReadTransform()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray transform = []; 
            int count = reader.ReadVarInt();
            for (int i = 0, bonesCount; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                JsonArray bonesArray = []; bonesCount = reader.ReadVarInt();
                for (int j = 0; j < bonesCount; j++) bonesArray.Add(bones[reader.ReadVarInt()]["name"].GetValue<string>());
                data["bones"] = bonesArray;
                data["target"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["local"] = reader.ReadBoolean();
                data["relative"] = reader.ReadBoolean();
                data["rotation"] = reader.ReadFloat();
                data["x"] = reader.ReadFloat();
                data["y"] = reader.ReadFloat();
                data["scaleX"] = reader.ReadFloat();
                data["scaleY"] = reader.ReadFloat();
                data["shearY"] = reader.ReadFloat();
                data["rotateMix"] = reader.ReadFloat();
                data["translateMix"] = reader.ReadFloat();
                data["scaleMix"] = reader.ReadFloat();
                data["shearMix"] = reader.ReadFloat();
                transform.Add(data);
            }
            return transform;
        }

        private JsonArray ReadPath()
        {
            JsonArray bones = root["bones"].AsArray();
            JsonArray path = []; 
            int count = reader.ReadVarInt();
            for (int i = 0, bonesCount; i < count; i++)
            {
                JsonObject data = [];
                data["name"] = reader.ReadString();
                data["order"] = reader.ReadVarInt();
                data["skin"] = reader.ReadBoolean();
                JsonArray bonesArray = []; bonesCount = reader.ReadVarInt();
                for (int j = 0; j < bonesCount; j++) bonesArray.Add(bones[reader.ReadVarInt()]["name"].GetValue<string>());
                data["bones"] = bonesArray;
                data["target"] = bones[reader.ReadVarInt()]["name"].GetValue<string>();
                data["positionMode"] = ((PositionMode)reader.ReadVarInt()).ToString();
                data["spacingMode"] = ((SpacingMode)reader.ReadVarInt()).ToString();
                data["rotateMode"] = ((RotateMode)reader.ReadVarInt()).ToString();
                data["rotation"] = reader.ReadFloat();
                data["position"] = reader.ReadFloat();
                data["spacing"] = reader.ReadFloat();
                data["rotateMix"] = reader.ReadFloat();
                data["translateMix"] = reader.ReadFloat();
                path.Add(data);
            }
            return path;
        }

        private JsonArray ReadSkins()
        {
            JsonArray skins = []; 
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                throw new NotImplementedException();
            }
            return skins;
        }

        private JsonObject ReadEvents()
        {
            JsonObject events = [];
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                JsonObject data = [];
                events[reader.ReadStringRef()] = data;
                data["int"] = reader.ReadVarInt(false);
                data["float"] = reader.ReadFloat();
                data["string"] = reader.ReadString();
                string audio = reader.ReadString();
                if (audio is not null)
                {
                    data["audio"] = audio;
                    data["volume"] = reader.ReadFloat();
                    data["balance"] = reader.ReadFloat();
                }
            }
            return events;
        }

        private JsonObject ReadAnimations()
        {
            JsonObject animations = [];
            int count = reader.ReadVarInt();
            for (int i = 0; i < count; i++)
            {
                throw new NotImplementedException();
            }
            return animations;
        }

        protected override void WriteBinary(JsonObject root, string binPath)
        {
            throw new NotImplementedException();
        }

    }
}
