using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using System.Linq.Expressions;

namespace EntityFrameworkMerge
{
    public class ClassA
    {
        public int MyProperty { get; set; } = 1;
        public int MyProperty1 { get; set; } = 2;
    }

    public class ClassB
    {
        public int MyProperty { get; set; } = 1;
        public int MyProperty1 { get; set; } = 3;
    }

    public class Entity1
    {
        public Entity1()
        {
            Id = Guid.NewGuid();
        }
        //public Entity1(int id, string name)
        //{
        //    Id = Guid.NewGuid();
        //    DepId = id;
        //    Name = name;
        //}
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
        //public Entity2(int id, string name)
        //{
        //    Id = Guid.NewGuid();
        //    DepId = id;
        //    Name = name;
        //    FullName = name + '/' + name;
        //}
        public Guid Id { get; set; }
        public int DepId { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            AutoMapper.Mappers.MapperRegistry.Reset();
            var autoMapperCfg = new AutoMapper.ConfigurationStore(new TypeMapFactory(), AutoMapper.Mappers.MapperRegistry.Mappers);
            var mappingEngine = new AutoMapper.MappingEngine(autoMapperCfg);
            autoMapperCfg.Seal();

            autoMapperCfg.CreateMap<Entity1, Entity2>().ReverseMap();
            var a1 = new Entity1();
            var b1 = mappingEngine.Map<Entity2>(a1);

            var colEntity1 = new List<Entity1> { new Entity1() { DepId = 1, Name = "abc"}, new Entity1() { DepId = 2, Name = "qwe" }, new Entity1() { DepId = 3, Name = "qaz" } };

            var colEntity2 = new List<Entity2> { new Entity2() { DepId = 2, Name = "newNameFirst" }, new Entity2() { DepId = 3, Name = "newNameSecond" }, new Entity2() { DepId = 4, Name = "newName" } };
            //Mapper.Initialize(cfg => cfg.CreateMap<Entity1, Entity2>());
            CompareCollections<Entity1, Entity2>.Compare(colEntity1, colEntity2, x => x.DepId);
        }
        
    
    }
    public static class CompareCollections<T, V> 
    {
        public static void Compare(ICollection<T> BaseCollection, ICollection<V> IncomingCollection, Func<T, object> comparingProperty)
        {
            Expression<Func<T, object>> expr = x => comparingProperty(x);

            Func<V, object> comparingPropertyMappedType = FuncInputTypeConverter<V, object, T>.Map(comparingProperty, x => Mapper.DynamicMap<T>(x));

            var ToDelete = BaseCollection.Where(
                x => !IncomingCollection.Select(comparingPropertyMappedType).Contains(comparingProperty(x)));
            //x => IncomingCollection.Any(
            //    y => 
            //    comparingPropertyMappedType(y) == comparingProperty(x)));
            //y => comparingPropertyMappedType(y)

            ToDelete.ToList().ForEach(x => BaseCollection.Remove(x));
            
            

            foreach (T baseEntity in BaseCollection)
            {
                var IncomingEntity = IncomingCollection.Where(GetComparsion(x, ExpressionType.Equal, expr.Parameters.Select(y => y.Name).SingleOrDefault(), comparingProperty(baseEntity)));
                Mapper.Map(IncomingEntity, baseEntity);
            }

            var ToInsert = BaseCollection.Where(
                x => IncomingCollection.All(
                    y =>
                    comparingPropertyMappedType(y) != comparingProperty(x)));

            ToInsert.ToList().ForEach(x => BaseCollection.Add(x));
        }

        private static Expression<Func<V, bool>> GetComparsion(V parameter, ExpressionType expType, string parameterName, object constVal)
        {
            var lhPar = Expression.Parameter(typeof(V), parameterName);
            var rhPar = Expression.Constant(constVal);

            var binaryExpr = Expression.MakeBinary(expType, lhPar, rhPar);
            var lambda = Expression.Lambda<Func<V, bool>>(binaryExpr, lhPar);
            return lambda;
        }
        
        
    }

    public static class AutoMapperGenericsHelper<TSource, TDestination>
    {
        public static TDestination ConvertToDestinationType(TSource model)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<TSource, TDestination>());
            return Mapper.Map<TSource, TDestination>(model);
        }

        static Func<TNewIn, TOut> Map<TOrigIn, TNewIn, TOut>(Func<TOrigIn, TOut> input, Func<TNewIn, TOrigIn> convert)
        {
            return x => input(convert(x));
        }
    }

    public static class FuncInputTypeConverter<TNewInput, TOutput, TOrigInput>
    {
        public static Func<TNewInput, TOutput> Map(Func<TOrigInput, TOutput> input, Func<TNewInput, TOrigInput> convert)
        {
            return x => input(convert(x));
        }
    }


    



    //public class DomainToViewModelMappingProfile : Profile
    //{

    //    public DomainToViewModelMappingProfile()
    //    {
    //        ConfigureMappings();
    //    }


    //    /// <summary>
    //    /// Creates a mapping between source (Domain) and destination (ViewModel)
    //    /// </summary>
    //    private void ConfigureMappings()
    //    {
    //        Mapper.CreateMap<Entity1, Entity2>().ReverseMap();
    //        //CreateMap<Entity2, Entity1>();
    //        //CreateMap<Entity1, Entity2>();
    //    }
    //}
}
