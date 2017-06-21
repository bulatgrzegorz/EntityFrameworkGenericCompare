using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace EntityFrameworkMerge
{
    class Program
    {
        static void Main(string[] args)
        {
            var colEntity1 = new List<Entity1> { new Entity1(), new Entity1() };

            var colEntity2 = new List<Entity2> { new Entity2(), new Entity2() };
            Mapper.Initialize(cfg => cfg.CreateMap<Entity1, Entity2>());
            CompareCollections<Entity1, Entity2>.Compare(colEntity1, colEntity2, x => x.Id);
        }
        
    
    }
    public static class CompareCollections<T, V> 
    {
        public static void Compare(ICollection<T> BaseCollection, ICollection<V> IncomingCollection, Func<T, object> comparingProperty)
        {
            var ToDelete = BaseCollection.Where(
                x => !IncomingCollection.Any(
                    y => 
                    comparingProperty(AutoMapperGenericsHelper<V, T>.ConvertToDestinationType(y)) == comparingProperty(x)));

            ToDelete.ToList().ForEach(x => BaseCollection.Remove(x));



            var ToInsert = BaseCollection.Where(
                x => IncomingCollection.All(
                    y =>
                    comparingProperty(AutoMapperGenericsHelper<V, T>.ConvertToDestinationType(y)) != comparingProperty(x)));

            
            ToInsert.ToList().ForEach(x => BaseCollection.Add(x));

            BaseCollection.ToList().RemoveAll(x => !IncomingCollection.Any(
                    y => 
                    comparingProperty(AutoMapperGenericsHelper<V, T>.ConvertToDestinationType(y)) == comparingProperty(x)));

            var toInsert = BaseCollection.ToList().Where(x => IncomingCollection.All(
                y =>
                comparingProperty(AutoMapperGenericsHelper<V, T>.ConvertToDestinationType(y)) != comparingProperty(x)));
        }

        
    }

    public static class AutoMapperGenericsHelper<TSource, TDestination>
    {
        public static TDestination ConvertToDestinationType(TSource model)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<TSource, TDestination>());
            return Mapper.Map<TSource, TDestination>(model);
        }
    }


    public class Entity1
    {
        public Entity1()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public int DepId { get; set; }
        public string Name { get; set; }
    }

    public class Entity2
    {
        public Entity2()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public int DepId { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
    }
}
