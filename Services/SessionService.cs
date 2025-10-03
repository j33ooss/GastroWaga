using GastroWaga.Data;
using GastroWaga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GastroWaga.Services
{
    public static class AppState
    {
        public static Guid? CurrentSessionId { get; set; }
        public static string? CurrentSessionName { get; set; }
    }

    public class SessionService
    {
        public async Task<List<Session>> GetOpenAsync()
        {
            using var db = new AppDbContext();
            return await db.Sessions
                .Where(s => s.Status == SessionStatus.Open)
                .OrderByDescending(s => s.LastModifiedAt)
                .ToListAsync();
        }

        public async Task<List<Session>> GetClosedAsync()
        {
            using var db = new AppDbContext();
            return await db.Sessions
                .Where(s => s.Status == SessionStatus.Closed)
                .OrderByDescending(s => s.ClosedAt)
                .ToListAsync();
        }

        public async Task<Session> CreateAsync(string name, string warehouse, string user)
        {
            using var db = new AppDbContext();
            var s = new Session
            {
                Name = string.IsNullOrWhiteSpace(name)
                    ? $"{warehouse} – {DateTime.Now:yyyy-MM-dd HH:mm}"
                    : name.Trim(),
                Warehouse = warehouse ?? "",
                User = user ?? "",
                Status = SessionStatus.Open
            };
            db.Sessions.Add(s);
            await db.SaveChangesAsync();

            AppState.CurrentSessionId = s.Id;
            AppState.CurrentSessionName = s.Name;
            return s;
        }

        public async Task CloseAsync(Guid sessionId, string? by = null)
        {
            using var db = new AppDbContext();
            var s = await db.Sessions.FirstAsync(x => x.Id == sessionId);
            s.Status = SessionStatus.Closed;
            s.ClosedAt = DateTime.UtcNow;
            s.LastModifiedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            if (AppState.CurrentSessionId == sessionId)
            {
                AppState.CurrentSessionId = null;
                AppState.CurrentSessionName = null;
            }
        }

        public async Task ReopenAsync(Guid sessionId)
        {
            using var db = new AppDbContext();
            var s = await db.Sessions.FirstAsync(x => x.Id == sessionId);
            s.Status = SessionStatus.Open;
            s.ClosedAt = null;
            s.LastModifiedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task<Session> DuplicateAsync(Guid sessionId)
        {
            using var db = new AppDbContext();
            var src = await db.Sessions.FirstAsync(x => x.Id == sessionId);
            var copy = new Session
            {
                Name = src.Name + " (kopiuj)",
                Warehouse = src.Warehouse,
                User = src.User,
                Status = SessionStatus.Open
            };
            db.Sessions.Add(copy);
            await db.SaveChangesAsync();
            return copy;
        }
    }
}
