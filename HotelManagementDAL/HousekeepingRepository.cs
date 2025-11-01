using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class HousekeepingRepository : IHousekeepingRepository
{
    public async Task<IReadOnlyList<HousekeepingTask>> GetAllAsync(string connectionString, CancellationToken ct = default)
    {
        var list = new List<HousekeepingTask>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select h.TaskId, h.RoomId, r.RoomNumber, h.TaskDate, h.StaffName, h.Status, h.Notes
from Housekeeping h join Rooms r on r.RoomId = h.RoomId order by h.TaskDate desc, h.TaskId desc";
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new HousekeepingTask
            {
                TaskId = rd.GetInt32(0),
                RoomId = rd.GetInt32(1),
                RoomNumber = rd.GetString(2),
                TaskDate = rd.GetDateTime(3),
                StaffName = await rd.IsDBNullAsync(4, ct) ? null : rd.GetString(4),
                Status = rd.GetString(5),
                Notes = await rd.IsDBNullAsync(6, ct) ? null : rd.GetString(6)
            });
        }
        return list;
    }

    public async Task<HousekeepingTask?> GetByIdAsync(string connectionString, int taskId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select TaskId, RoomId, TaskDate, StaffName, Status, Notes from Housekeeping where TaskId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = taskId });
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await rd.ReadAsync(ct))
        {
            return new HousekeepingTask
            {
                TaskId = rd.GetInt32(0),
                RoomId = rd.GetInt32(1),
                TaskDate = rd.GetDateTime(2),
                StaffName = await rd.IsDBNullAsync(3, ct) ? null : rd.GetString(3),
                Status = rd.GetString(4),
                Notes = await rd.IsDBNullAsync(5, ct) ? null : rd.GetString(5)
            };
        }
        return null;
    }

    public async Task<int> AddAsync(string connectionString, HousekeepingTask task, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"insert into Housekeeping(RoomId, TaskDate, StaffName, Status, Notes)
values(@RoomId,@TaskDate,@StaffName,@Status,@Notes);
select cast(SCOPE_IDENTITY() as int);";
        cmd.Parameters.Add(new SqlParameter("@RoomId", SqlDbType.Int) { Value = task.RoomId });
        cmd.Parameters.Add(new SqlParameter("@TaskDate", SqlDbType.Date) { Value = task.TaskDate.Date });
        cmd.Parameters.Add(new SqlParameter("@StaffName", SqlDbType.NVarChar, 150) { Value = (object?)task.StaffName ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 30) { Value = task.Status });
        cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 300) { Value = (object?)task.Notes ?? DBNull.Value });
        var id = (int)await cmd.ExecuteScalarAsync(ct);
        return id;
    }

    public async Task<bool> UpdateAsync(string connectionString, HousekeepingTask task, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"update Housekeeping set RoomId=@RoomId, TaskDate=@TaskDate, StaffName=@StaffName, Status=@Status, Notes=@Notes where TaskId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = task.TaskId });
        cmd.Parameters.Add(new SqlParameter("@RoomId", SqlDbType.Int) { Value = task.RoomId });
        cmd.Parameters.Add(new SqlParameter("@TaskDate", SqlDbType.Date) { Value = task.TaskDate.Date });
        cmd.Parameters.Add(new SqlParameter("@StaffName", SqlDbType.NVarChar, 150) { Value = (object?)task.StaffName ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 30) { Value = task.Status });
        cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 300) { Value = (object?)task.Notes ?? DBNull.Value });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string connectionString, int taskId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"delete from Housekeeping where TaskId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = taskId });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }
}
