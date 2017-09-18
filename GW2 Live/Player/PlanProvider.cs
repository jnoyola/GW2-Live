using System;
using System.Threading.Tasks;

namespace GW2_Live.Player
{
    public interface IPlanProvider
    {
        Task<Plan.Serializable> Load(int mapId);
        void Save(Plan.Serializable plan, int mapId);
    }

    class PlanProvider : IPlanProvider
    {
        public static PlanProvider Instance = new PlanProvider();

        private static readonly string Folder = "plans";
        private static readonly string FileFormat = "{0}.json";

        public async Task<Plan.Serializable> Load(int mapId)
        {
            return await FileManager.ReadFromFile<Plan.Serializable>(Folder, String.Format(FileFormat, mapId));
        }

        public void Save(Plan.Serializable plan, int mapId)
        {
            FileManager.SaveToFile(plan, Folder, String.Format(FileFormat, mapId));
        }
    }
}
