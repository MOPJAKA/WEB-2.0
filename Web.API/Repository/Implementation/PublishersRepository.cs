using WEB.API.Context;
using Microsoft.EntityFrameworkCore;
using WEB.API.DTO;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Repository.Implementation
{
    public class PublishersRepository : IPublishersRepository
    {
        private readonly ApplicationDBContext _context;

        public PublishersRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Publishers>> GetAllAsync()
        {
            return await _context.Publishers
                .ToListAsync();
        }

        public async Task<Publishers> GetAsync(int id)
        {
            return await _context.Publishers
                .FirstOrDefaultAsync(publisher => publisher.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Publishers publisher = await _context.Publishers.FirstOrDefaultAsync(publisher => publisher.Id == id);

            if (publisher == null)
            {
                return false;
            }

            _context.Publishers.Remove(publisher);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Publishers> CreateAsync(PublishersCreateDTO DTO)
        {
            // Создаем новый объект Publishers на основе DTO
            var publisher = new Publishers
            {
                Name = DTO.Name
            };

            // Добавляем издателя в контекст
            await _context.Publishers.AddAsync(publisher);
            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return publisher; // Возвращаем созданного издателя
        }


        public async Task<Publishers> UpdateAsync(int id, PublishersUpdateDTO DTO)
        {
            // Находим издателя по id
            var publisher = await _context.Publishers.FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
            {
                return null; // Издатель не найден
            }

            // Обновляем данные издателя
            publisher.Name = DTO.Name;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return publisher; // Возвращаем обновленного издателя
        }
    }
}
