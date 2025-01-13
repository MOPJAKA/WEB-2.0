using Microsoft.EntityFrameworkCore;
using WEB.API.Models;

namespace WEB.API.Context
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }

        // DbSet - класс для формирования модели таблицы бд
        // DbSet<Authors>==list<int> - формирование множества
        public DbSet<Authors> Authors { get; set; }
        public DbSet<Books> Books { get; set; }
        public DbSet<Publishers> Publishers { get; set; }
        public DbSet<Readers> Readers { get; set; }
        public DbSet<BookIssues> BookIssues { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        // переопределяем функцию из наследуемого класса 
        {
            base.OnModelCreating(builder);


            // Настройка сущностей

            // Authors
            builder.Entity<Authors>()
                .HasKey(x => x.Id);

            builder.Entity<Authors>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();


            // Books
            builder.Entity<Books>()
                .HasKey(x => x.Id);

            builder.Entity<Books>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
            // как Serial
            // настройки поведения модели данных, их свойств и связей между сущностями

            builder.Entity<Books>()
                .HasOne(x => x.Author) // к одному автору 
                .WithMany() // много книг
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            // не удаляем ничего, если есть дочерние ссылки
            // нужно удалять дочерние самостоятельно

            builder.Entity<Books>()
                .HasOne(x => x.Publisher)
                .WithMany()
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Publishers
            builder.Entity<Publishers>()
                .HasKey(x => x.Id);

            builder.Entity<Publishers>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();


            // Readers
            builder.Entity<Readers>()
                .HasKey(x => x.Id);

            builder.Entity<Readers>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();


            // BookIssues
            builder.Entity<BookIssues>()
                .HasKey(x => x.Id);

            builder.Entity<BookIssues>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<BookIssues>()
                .Property(x => x.ActualReturnDate)
                .IsRequired(false);

            builder.Entity<BookIssues>()
                .HasOne(x => x.Reader)
                .WithMany()
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BookIssues>()
                .HasOne(x => x.Book)
                .WithMany()
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
