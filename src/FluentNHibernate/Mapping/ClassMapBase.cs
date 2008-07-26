using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using ShadeTree.Core;

namespace ShadeTree.DomainModel.Mapping
{
    public class ClassMapBase<T>
    {
        private bool _parentIsRequired = true;
        
        protected readonly List<IMappingPart> _properties = new List<IMappingPart>();


        protected bool parentIsRequired
        {
            get { return _parentIsRequired; }
            set { _parentIsRequired = value; }
        }

        protected void addPart(IMappingPart part)
        {
            _properties.Add(part);
        }

        public PropertyMap Map(Expression<Func<T, object>> expression)
        {
            return Map(expression, null);
        }

        public PropertyMap Map(Expression<Func<T,object>> expression, string columnName)
        {
            PropertyInfo property = ReflectionHelper.GetProperty(expression);
            var map = new PropertyMap(property, parentIsRequired, columnName ?? property.Name, typeof(T));

            _properties.Add(map);

            return map;
        }

        public ManyToOnePart References(Expression<Func<T, object>> expression)
        {
            PropertyInfo property = ReflectionHelper.GetProperty(expression);
            ManyToOnePart part = new ManyToOnePart(property);
            addPart(part);

            return part;
        }

        public DiscriminatorPart<ARG, T> DiscriminateSubClassesOnColumn<ARG>(string columnName)
        {
            var part = new DiscriminatorPart<ARG, T>(columnName, _properties);
            addPart(part);

            return part;
        }

        public void Component<C>(Expression<Func<T, object>> expression, Action<ComponentPart<C>> action)
        {
            PropertyInfo property = ReflectionHelper.GetProperty(expression);

            ComponentPart<C> part = new ComponentPart<C>(property, parentIsRequired);
            addPart(part);

            action(part);
        }

        public OneToManyPart<T, CHILD> HasMany<CHILD>(Expression<Func<T, object>> expression)
        {
            PropertyInfo property = ReflectionHelper.GetProperty(expression);
            OneToManyPart<T, CHILD> part = new OneToManyPart<T, CHILD>(property);

            addPart(part);

            return part;
        }

        public ManyToManyPart<T, CHILD> HasManyToMany<CHILD>(Expression<Func<T, object>> expression)
        {
            PropertyInfo property = ReflectionHelper.GetProperty(expression);
            ManyToManyPart<T, CHILD> part = new ManyToManyPart<T, CHILD>(property);

            addPart(part);

            return part;
        }

        protected void writeTheParts(XmlElement classElement, IMappingVisitor visitor)
        {
            _properties.Sort(new MappingPartComparer());
            foreach (IMappingPart part in _properties)
            {
                part.Write(classElement, visitor);
            }
        }

        internal class MappingPartComparer : IComparer<IMappingPart>
        {
            public int Compare(IMappingPart x, IMappingPart y)
            {
                return x.Level.CompareTo(y.Level);
            }
        }
    }
}