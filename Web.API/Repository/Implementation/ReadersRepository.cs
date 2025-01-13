using WEB.API.Context;
using Microsoft.EntityFrameworkCore;
using WEB.API.DTO;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Repository.Implementation
{
    public class ReadersRepository : IReadersRepository
    {
        private readonly ApplicationDBContext _context;

        public ReadersRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Readers>> GetAllAsync()
        {
            return await _context.Readers
                .ToListAsync();
        }

        public async Task<Readers> GetAsync(int id)
        {
            return await _context.Readers
                .FirstOrDefaultAsync(reader => reader.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Readers reader = await _context.Readers.FirstOrDefaultAsync(reader => reader.Id == id);

            if (reader == null)
            {
                return false;
            }

            _context.Readers.Remove(reader);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Readers> UpdateAsync(int id, ReadersUpdateDTO DTO)
        {
            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.Id == id);

            if (reader == null)
            {
                return null;  // Если читатель не найден, возвращаем null
            }

            // Обновляем данные читателя
            reader.FirstName = DTO.FirstName;
            reader.LastName = DTO.LastName;
            reader.BirthDayDate = DTO.BirthDayDate;
            reader.Gender = DTO.Gender;
            reader.EducationLevel = DTO.EducationLevel;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return reader;  // Возвращаем обновленного читателя
        }

        public async Task<Readers> CreateAsync(ReadersCreateDTO DTO)
        {
            var reader = new Readers
            {
                FirstName = DTO.FirstName,
                LastName = DTO.LastName,
                BirthDayDate = DTO.BirthDayDate,
                Gender = DTO.Gender,
                EducationLevel = DTO.EducationLevel
            };

            // Добавляем нового читателя в контекст
            await _context.Readers.AddAsync(reader);
            await _context.SaveChangesAsync();

            return reader;
        }
    }
}
