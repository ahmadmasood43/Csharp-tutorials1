using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

// Step 1: Define your entities
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    // other properties
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    // other properties
}

// Define the other entities similarly

// Step 2: Create a DbContext
public class YourDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    // other DbSets

    public YourDbContext(DbContextOptions<YourDbContext> options)
        : base(options)
    {
    }
}

// Step 3: Define generic interfaces for repositories
public interface IRepository<T>
{
    IQueryable<T> GetAll();
    T GetById(int id);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// Step 4: Implement generic repository
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly YourDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(YourDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> GetAll()
    {
        return _dbSet;
    }

    public T GetById(int id)
    {
        return _dbSet.Find(id);
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }
}

// Step 5: Create a Unit of Work interface
public interface IUnitOfWork : IDisposable
{
    IRepository<Customer> CustomerRepository { get; }
    IRepository<Product> ProductRepository { get; }
    // add repositories for other entities

    void Save();
}

// Step 6: Implement the Unit of Work
public class UnitOfWork : IUnitOfWork
{
    private readonly YourDbContext _context;

    public UnitOfWork(YourDbContext context)
    {
        _context = context;
        CustomerRepository = new Repository<Customer>(_context);
        ProductRepository = new Repository<Product>(_context);
        // initialize repositories for other entities
    }

    public IRepository<Customer> CustomerRepository { get; }
    public IRepository<Product> ProductRepository { get; }
    // add repositories for other entities

    public void Save()
    {
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

// Example of usage
class Program
{
    static void Main()
    {
        var options = new DbContextOptionsBuilder<YourDbContext>()
            .UseInMemoryDatabase("InMemoryDatabase")
            .Options;

        using (var unitOfWork = new UnitOfWork(new YourDbContext(options)))
        {
            var customer = new Customer { Name = "John Doe" };
            unitOfWork.CustomerRepository.Add(customer);

            var product = new Product { Name = "Sample Product" };
            unitOfWork.ProductRepository.Add(product);

            // Add/update/delete other entities as needed

            unitOfWork.Save();
        }
    }
}

