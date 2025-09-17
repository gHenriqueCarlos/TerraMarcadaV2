using Maui.GoogleMaps;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraMarcadaV2.Models;
//using static CoreFoundation.DispatchSource;

namespace TerraMarcadaV2.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _connection;

        public DatabaseService(string dbPath)
        {
            _connection = new SQLiteAsyncConnection(dbPath);
            _connection.CreateTableAsync<MapData>().Wait();
        }

        public async Task<int> AddMapData(MapData data)
        {
            await _connection.InsertAsync(data);
            return data.Id;
        }

        // Atualizar
        public async Task<int> UpdateMapData(MapData data)
        {
            data.UpdatedAtUtc = DateTime.UtcNow;
            return await _connection.UpdateAsync(data);
        }

        // Buscar por ID
        public async Task<MapData> GetMapDataById(int id)
        {
            return await _connection.FindAsync<MapData>(id);
        }

        // Buscar todos
        public async Task<List<MapData>> GetAllMapData()
        {
            return await _connection.Table<MapData>().ToListAsync();
        }

        // Deletar
        public async Task<int> DeleteMapData(int id)
        {
            var item = await _connection.FindAsync<MapData>(id);
            if (item != null)
                return await _connection.DeleteAsync(item);
            return 0;
        }

        // apaga todos os holes de um polígono pai
        public async Task<int> DeleteHolesByParentIdAsync(int polygonId)
        {
            var holes = await _connection.Table<MapData>()
                .Where(d => d.Type == MapDataTypes.Hole && d.ParentId == polygonId)
                .ToListAsync();

            int count = 0;
            foreach (var h in holes)
                count += await _connection.DeleteAsync(h);

            return count;
        }

        public async Task<int> DeleteHoleByParentAndCoordsAsync(int polygonId, IEnumerable<Position> coords)
        {
            var target = GeoUtils.Canonicalize(coords);

            var holes = await _connection.Table<MapData>()
                .Where(d => d.Type == MapDataTypes.Hole && d.ParentId == polygonId)
                .ToListAsync();

            var toDelete = holes.Where(h => GeoUtils.Canonicalize(h.GetCoordinates()) == target).ToList();

            int count = 0;
            foreach (var h in toDelete)
                count += await _connection.DeleteAsync(h);

            return count;
        }

        // Deletar todos
        public async Task<int> DeleteAll()
        {
            return await _connection.DeleteAllAsync<MapData>();
        }
    }
}