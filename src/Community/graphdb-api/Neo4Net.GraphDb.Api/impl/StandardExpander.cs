using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace Neo4Net.GraphDb.impl
{
   using Neo4Net.GraphDb;
   using Neo4Net.GraphDb.Exceptions;
   using Neo4Net.GraphDb.Traversal;

   public abstract class StandardExpander : IPathExpander
   {
      private StandardExpander()
      {
      }

      internal abstract class StandardExpansion<T> : ResourceIterable<T>
      {
         public abstract java.util.stream.Stream<T> Stream();

         public abstract ResourceIterator<T> Iterator();

         internal readonly StandardExpander _expander;
         internal readonly IPath _path;
         internal readonly IBranchState<T> _state;

         internal StandardExpansion(StandardExpander expander, IPath path, IBranchState<T> state)
         {
            _expander = expander;
            _path = path;
            _state = state;
         }

         internal virtual string StringRepresentation(string nodesORrelationships)
         {
            return "Expansion[" + _path + ".expand( " + _expander + " )." + nodesORrelationships + "()]";
         }

         internal abstract StandardExpansion<T> CreateNew(StandardExpander expander);

         public virtual StandardExpansion<T> Including(IRelationshipType type)
         {
            return CreateNew(_expander.Add(type));
         }

         public virtual StandardExpansion<T> Including(IRelationshipType type, Direction direction)
         {
            return CreateNew(_expander.add(type, direction));
         }

         public virtual StandardExpansion<T> Excluding(IRelationshipType type)
         {
            return CreateNew(_expander.remove(type));
         }

         //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
         //ORIGINAL LINE: public StandardExpansion<T> filterNodes(System.Predicate<? super org.Neo4Net.graphdb.Node> filter)
         public virtual StandardExpansion<T> FilterNodes<T1>(System.Predicate<T1> filter)
         {
            return CreateNew(_expander.addNodeFilter(filter));
         }

         //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
         //ORIGINAL LINE: public StandardExpansion<T> filterRelationships(System.Predicate<? super org.Neo4Net.graphdb.Relationship> filter)
         public virtual StandardExpansion<T> FilterRelationships<T1>(System.Predicate<T1> filter)
         {
            return CreateNew(_expander.AddRelationshipFilter(filter));
         }

         public virtual T Single
         {
            get
            {
               //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
               //ORIGINAL LINE: final java.util.Iterator<T> expanded = iterator();
               IEnumerator<T> expanded = Iterator();
               //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
               if (expanded.hasNext())
               {
                  //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                  //ORIGINAL LINE: final T result = expanded.next();
                  //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                  T result = expanded.next();
                  //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                  if (expanded.hasNext())
                  {
                     throw new NotFoundException("More than one relationship found for " + this);
                  }
                  return result;
               }
               return default(T);
            }
         }

         public virtual bool Empty
         {
            get
            {
               //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
               return !_expander.doExpand(_path, _state).hasNext();
            }
         }

         public virtual StandardExpansion<INode> Nodes()
         {
            return new NodeExpansion(_expander, _path, _state);
         }

         public virtual StandardExpansion<IRelationship> Relationships()
         {
            return new RelationshipExpansion(_expander, _path, _state);
         }
      }

      private sealed class RelationshipExpansion : StandardExpansion<IRelationship>
      {
         internal RelationshipExpansion(StandardExpander expander, IPath path, IBranchState state) : base(expander, path, state)
         {
         }

         public override string ToString()
         {
            return StringRepresentation("relationships");
         }

         internal override StandardExpansion<IRelationship> CreateNew(StandardExpander expander)
         {
            return new RelationshipExpansion(expander, _path, _state);
         }

         public override StandardExpansion<IRelationship> Relationships()
         {
            return this;
         }

         public override ResourceIterator<IRelationship> Iterator()
         {
            return _expander.doExpand(_path, _state);
         }
      }

      private sealed class NodeExpansion : StandardExpansion<INode>
      {
         internal NodeExpansion(StandardExpander expander, IPath path, IBranchState state) : base(expander, path, state)
         {
         }

         public override string ToString()
         {
            return StringRepresentation("nodes");
         }

         internal override StandardExpansion<INode> CreateNew(StandardExpander expander)
         {
            return new NodeExpansion(expander, _path, _state);
         }

         public override StandardExpansion<INode> Nodes()
         {
            return this;
         }

         public override ResourceIterator<INode> Iterator()
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final org.Neo4Net.graphdb.Node node = path.endNode();
            INode node = _path.endNode();

            return new MappingResourceIteratorAnonymousInnerClass(this, _expander.doExpand(_path, _state), node);
         }

         private class MappingResourceIteratorAnonymousInnerClass : MappingResourceIterator<INode, IRelationship>
         {
            private readonly NodeExpansion _outerInstance;

            private INode _node;

            public MappingResourceIteratorAnonymousInnerClass(NodeExpansion outerInstance, ResourceIterator<IRelationship> doExpand, INode node) : base(doExpand)
            {
               this.outerInstance = outerInstance;
               _node = node;
            }

            protected internal override INode map(IRelationship rel)
            {
               return rel.GetOtherNode(_node);
            }
         }
      }

      private class AllExpander : StandardExpander
      {
         internal readonly Direction Direction;

         internal AllExpander(Direction direction)
         {
            this.Direction = direction;
         }

         internal override void BuildString(StringBuilder result)
         {
            if (Direction != Direction.BOTH)
            {
               result.Append(Direction);
               result.Append(":");
            }
            result.Append("*");
         }

         internal override ResourceIterator<IRelationship> DoExpand(IPath path, IBranchState state)
         {
            return asResourceIterator(path.EndNode.getRelationships(Direction).GetEnumerator());
         }

         public override StandardExpander Add(IRelationshipType type, Direction dir)
         {
            return this;
         }

         public override StandardExpander Remove(IRelationshipType type)
         {
            IDictionary<string, Exclusion> exclude = new Dictionary<string, Exclusion>();
            exclude[type.Name()] = Exclusion.All;
            return new ExcludingExpander(Exclusion.include(Direction), exclude);
         }

         public override StandardExpander Reversed()
         {
            return Reverse();
         }

         public override StandardExpander Reverse()
         {
            return new AllExpander(Direction.reverse());
         }
      }

      private sealed class Exclusion
      {
         //JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
         //           ALL(null, "!") { public boolean accept(org.Neo4Net.graphdb.Node start, org.Neo4Net.graphdb.Relationship rel) { return false; } },
         //JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
         //           INCOMING(org.Neo4Net.graphdb.Direction.OUTGOING) { Exclusion reversed() { return OUTGOING; } },
         //JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
         //           OUTGOING(org.Neo4Net.graphdb.Direction.INCOMING) { Exclusion reversed() { return INCOMING; } },
         //JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
         //           NONE(org.Neo4Net.graphdb.Direction.BOTH, "") { boolean includes(org.Neo4Net.graphdb.Direction direction) { return true; } };

         private static readonly IList<Exclusion> valueList = new List<Exclusion>();

         static Exclusion()
         {
            valueList.Add(ALL);
            valueList.Add(INCOMING);
            valueList.Add(OUTGOING);
            valueList.Add(NONE);
         }

         public enum InnerEnum
         {
            ALL,
            INCOMING,
            OUTGOING,
            NONE
         }

         public readonly InnerEnum innerEnumValue;
         private readonly string nameValue;
         private readonly int ordinalValue;
         private static int nextOrdinal = 0;

         private Exclusion(string name, InnerEnum innerEnum)
         {
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
         }

         internal readonly string @string;
         internal readonly Neo4Net.GraphDb.Direction direction;

         internal Exclusion(string name, InnerEnum innerEnum, Neo4Net.GraphDb.Direction direction, string @string)
         {
            _direction = direction;
            this.@string = @string;

            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
         }

         internal Exclusion(string name, InnerEnum innerEnum, Neo4Net.GraphDb.Direction direction)
         {
            _direction = direction;
            this.@string = "!" + name() + ":";

            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
         }

         public override string ToString()
         {
            return @string;
         }

         internal bool Accept(Neo4Net.GraphDb.INode start, Neo4Net.GraphDb.IRelationship rel)
         {
            return MatchDirection(_direction, start, rel);
         }

         internal Exclusion Reversed()
         {
            return this;
         }

         internal bool Includes(Neo4Net.GraphDb.Direction dir)
         {
            return _direction == dir;
         }

         internal static Exclusion Include(Neo4Net.GraphDb.Direction direction)
         {
            switch (direction._innerEnumValue)
            {
               case Direction.InnerEnum.INCOMING:
                  return OUTGOING;

               case Direction.InnerEnum.OUTGOING:
                  return INCOMING;

               default:
                  return NONE;
            }
         }

         public static IList<Exclusion> values()
         {
            return valueList;
         }

         public int ordinal()
         {
            return ordinalValue;
         }

         public static Exclusion ValueOf(string name)
         {
            foreach (Exclusion enumInstance in Exclusion.valueList)
            {
               if (enumInstance.nameValue == name)
               {
                  return enumInstance;
               }
            }
            throw new System.ArgumentException(name);
         }
      }

      private sealed class ExcludingExpander : StandardExpander
      {
         internal readonly Exclusion DefaultExclusion;
         internal readonly IDictionary<string, Exclusion> Exclusion;

         internal ExcludingExpander(Exclusion defaultExclusion, IDictionary<string, Exclusion> exclusion)
         {
            this.DefaultExclusion = defaultExclusion;
            this.Exclusion = exclusion;
         }

         internal override void BuildString(StringBuilder result)
         {
            // FIXME: not really correct
            result.Append(DefaultExclusion);
            result.Append("*");
            foreach (KeyValuePair<string, Exclusion> entry in Exclusion.SetOfKeyValuePairs())
            {
               result.Append(",");
               result.Append(entry.Value);
               result.Append(entry.Key);
            }
         }

         internal override ResourceIterator<IRelationship> DoExpand(IPath path, IBranchState state)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final org.Neo4Net.graphdb.Node node = path.endNode();
            INode node = path.EndNode;
            ResourceIterator<IRelationship> resourceIterator = asResourceIterator(node.Relationships.GetEnumerator());
            return newResourceIterator(new FilteringIterator<>(resourceIterator, rel =>
            {
               Exclusion exclude = Exclusion[rel.Type.name()];
               exclude = (exclude == null) ? DefaultExclusion : exclude;
               return exclude.accept(node, rel);
            }), resourceIterator);
         }

         public override StandardExpander Add(IRelationshipType type, Direction direction)
         {
            Exclusion excluded = Exclusion[type.Name()];
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.Map<String, Exclusion> newExclusion;
            IDictionary<string, Exclusion> newExclusion;
            if ((excluded == null ? DefaultExclusion : excluded).includes(direction))
            {
               return this;
            }
            else
            {
               excluded = Exclusion.include(direction);
               if (excluded == DefaultExclusion)
               {
                  if (Exclusion.Count == 1)
                  {
                     return new AllExpander(DefaultExclusion.direction);
                  }
                  else
                  {
                     newExclusion = new Dictionary<string, Exclusion>(Exclusion);
                     newExclusion.Remove(type.Name());
                  }
               }
               else
               {
                  newExclusion = new Dictionary<string, Exclusion>(Exclusion);
                  newExclusion[type.Name()] = excluded;
               }
            }
            return new ExcludingExpander(DefaultExclusion, newExclusion);
         }

         public override StandardExpander Remove(IRelationshipType type)
         {
            Exclusion excluded = Exclusion[type.Name()];
            if (excluded == Exclusion.All)
            {
               return this;
            }
            IDictionary<string, Exclusion> newExclusion = new Dictionary<string, Exclusion>(Exclusion);
            newExclusion[type.Name()] = Exclusion.All;
            return new ExcludingExpander(DefaultExclusion, newExclusion);
         }

         public override StandardExpander Reversed()
         {
            return Reverse();
         }

         public override StandardExpander Reverse()
         {
            IDictionary<string, Exclusion> newExclusion = new Dictionary<string, Exclusion>();
            foreach (KeyValuePair<string, Exclusion> entry in Exclusion.SetOfKeyValuePairs())
            {
               newExclusion[entry.Key] = entry.Value.reversed();
            }
            return new ExcludingExpander(DefaultExclusion.reversed(), newExclusion);
         }
      }

      public static readonly StandardExpander DEFAULT = new AllExpanderAnonymousInnerClass();

      private class AllExpanderAnonymousInnerClass : AllExpander
      {
         public AllExpanderAnonymousInnerClass() : base(Direction.BOTH)
         {
         }

         public override StandardExpander add(IRelationshipType type, Direction direction)
         {
            return Create(type, direction);
         }
      }

      public static readonly StandardExpander Empty = new RegularExpander(Collections.emptyMap());

      private class DirectionAndTypes
      {
         internal readonly Direction Direction;
         internal readonly IRelationshipType[] Types;

         internal DirectionAndTypes(Direction direction, IRelationshipType[] types)
         {
            this.Direction = direction;
            this.Types = types;
         }
      }

      internal class RegularExpander : StandardExpander
      {
         internal readonly IDictionary<Direction, IRelationshipType[]> TypesMap;
         internal readonly DirectionAndTypes[] Directions;

         internal RegularExpander(IDictionary<Direction, IRelationshipType[]> types)
         {
            this.TypesMap = types;
            this.Directions = new DirectionAndTypes[types.Count];
            int i = 0;
            foreach (KeyValuePair<Direction, IRelationshipType[]> entry in types.SetOfKeyValuePairs())
            {
               this.Directions[i++] = new DirectionAndTypes(entry.Key, entry.Value);
            }
         }

         internal override void BuildString(StringBuilder result)
         {
            result.Append(TypesMap.ToString());
         }

         internal override ResourceIterator<IRelationship> DoExpand(IPath path, IBranchState state)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final org.Neo4Net.graphdb.Node node = path.endNode();
            INode node = path.EndNode;
            if (Directions.Length == 1)
            {
               DirectionAndTypes direction = Directions[0];
               return asResourceIterator(node.GetRelationships(direction.Direction, direction.Types).GetEnumerator());
            }
            else
            {
               return new NestingResourceIteratorAnonymousInnerClass(this, node);
            }
         }

         private class NestingResourceIteratorAnonymousInnerClass : NestingResourceIterator<IRelationship, DirectionAndTypes>
         {
            private readonly RegularExpander _outerInstance;

            private INode _node;

            public NestingResourceIteratorAnonymousInnerClass(RegularExpander outerInstance, INode node) : base(new ArrayIterator<>(outerInstance.Directions))
            {
               this.outerInstance = outerInstance;
               _node = node;
            }

            protected internal override ResourceIterator<IRelationship> createNestedIterator(DirectionAndTypes item)
            {
               return asResourceIterator(_node.getRelationships(item.Direction, item.Types).GetEnumerator());
            }
         }

         internal virtual StandardExpander CreateNew(IDictionary<Direction, IRelationshipType[]> types)
         {
            if (types.Count == 0)
            {
               return new AllExpander(Direction.BOTH);
            }
            return new RegularExpander(types);
         }

         public override StandardExpander Add(IRelationshipType type, Direction direction)
         {
            IDictionary<Direction, ICollection<IRelationshipType>> tempMap = TemporaryTypeMapFrom(TypesMap);
            tempMap[direction].Add(type);
            return CreateNew(ToTypeMap(tempMap));
         }

         public override StandardExpander Remove(IRelationshipType type)
         {
            IDictionary<Direction, ICollection<IRelationshipType>> tempMap = TemporaryTypeMapFrom(TypesMap);
            foreach (Direction direction in Direction.values())
            {
               tempMap[direction].remove(type);
            }
            return CreateNew(ToTypeMap(tempMap));
         }

         public override StandardExpander Reversed()
         {
            return Reverse();
         }

         public override StandardExpander Reverse()
         {
            IDictionary<Direction, ICollection<IRelationshipType>> tempMap = TemporaryTypeMapFrom(TypesMap);
            ICollection<IRelationshipType> @out = tempMap[Direction.OUTGOING];
            ICollection<IRelationshipType> @in = tempMap[Direction.INCOMING];
            tempMap[Direction.OUTGOING] = @in;
            tempMap[Direction.INCOMING] = @out;
            return CreateNew(ToTypeMap(tempMap));
         }
      }

      private sealed class FilteringExpander : StandardExpander
      {
         internal readonly StandardExpander Expander;
         internal readonly Filter[] Filters;

         internal FilteringExpander(StandardExpander expander, params Filter[] filters)
         {
            this.Expander = expander;
            this.Filters = filters;
         }

         internal override void BuildString(StringBuilder result)
         {
            Expander.buildString(result);
            result.Append("; filter:");
            foreach (Filter filter in Filters)
            {
               result.Append(" ");
               result.Append(filter);
            }
         }

         //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
         //ORIGINAL LINE: ResourceIterator<org.Neo4Net.graphdb.Relationship> doExpand(final org.Neo4Net.graphdb.Path path, org.Neo4Net.graphdb.traversal.BranchState state)
         internal override ResourceIterator<IRelationship> DoExpand(IPath path, IBranchState state)
         {
            ResourceIterator<IRelationship> resourceIterator = Expander.doExpand(path, state);
            return newResourceIterator(new FilteringIterator<>(resourceIterator, item =>
            {
               IPath extendedPath = ExtendedPath.Extend(path, item);
               foreach (Filter filter in Filters)
               {
                  if (filter.Exclude(extendedPath))
                  {
                     return false;
                  }
               }
               return true;
            }), resourceIterator);
         }

         //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
         //ORIGINAL LINE: public StandardExpander addNodeFilter(System.Predicate<? super org.Neo4Net.graphdb.Node> filter)
         public override StandardExpander AddNodeFilter<T1>(System.Predicate<T1> filter)
         {
            return new FilteringExpander(Expander, Append(Filters, new NodeFilter(filter)));
         }

         //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
         //ORIGINAL LINE: public StandardExpander addRelationshipFilter(System.Predicate<? super org.Neo4Net.graphdb.Relationship> filter)
         public override StandardExpander AddRelationshipFilter<T1>(System.Predicate<T1> filter)
         {
            return new FilteringExpander(Expander, Append(Filters, new RelationshipFilter(filter)));
         }

         public override StandardExpander Add(IRelationshipType type, Direction direction)
         {
            return new FilteringExpander(Expander.add(type, direction), Filters);
         }

         public override StandardExpander Remove(IRelationshipType type)
         {
            return new FilteringExpander(Expander.remove(type), Filters);
         }

         public override StandardExpander Reversed()
         {
            return Reverse();
         }

         public override StandardExpander Reverse()
         {
            return new FilteringExpander(Expander.reversed(), Filters);
         }
      }

      private sealed class WrappingExpander : StandardExpander
      {
         internal const string IMMUTABLE = "Immutable Expander ";
         internal readonly IPathExpander Expander;

         internal WrappingExpander(IPathExpander expander)
         {
            this.Expander = expander;
         }

         internal override void BuildString(StringBuilder result)
         {
            result.Append(Expander);
         }

         internal override ResourceIterator<IRelationship> DoExpand(IPath path, IBranchState state)
         {
            return asResourceIterator(Expander.expand(path, state).GetEnumerator());
         }

         public override StandardExpander Add(IRelationshipType type, Direction direction)
         {
            throw new System.NotSupportedException(IMMUTABLE + Expander);
         }

         public override StandardExpander Remove(IRelationshipType type)
         {
            throw new System.NotSupportedException(IMMUTABLE + Expander);
         }

         public override StandardExpander Reversed()
         {
            return Reverse();
         }

         public override StandardExpander Reverse()
         {
            throw new System.NotSupportedException(IMMUTABLE + Expander);
         }
      }

      private abstract class Filter
      {
         internal abstract bool Exclude(IPath path);
      }

      private sealed class NodeFilter : Filter
      {
         //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
         //ORIGINAL LINE: private final System.Predicate<? super org.Neo4Net.graphdb.Node> predicate;
         //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
         internal readonly System.Predicate<object> Predicate;

         //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
         //ORIGINAL LINE: NodeFilter(System.Predicate<? super org.Neo4Net.graphdb.Node> predicate)
         internal NodeFilter<T1>(System.Predicate<T1> predicate )
			  {
					this.Predicate = predicate;
			  }

      public override string ToString()
      {
         return Predicate.ToString();
      }

      internal override bool Exclude(IPath path)
      {
         return !Predicate.test(path.EndNode);
      }
   }

   private sealed class RelationshipFilter : Filter
   {
      //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
      //ORIGINAL LINE: private final System.Predicate<? super org.Neo4Net.graphdb.Relationship> predicate;
      //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
      internal readonly System.Predicate<object> Predicate;

      //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
      //ORIGINAL LINE: RelationshipFilter(System.Predicate<? super org.Neo4Net.graphdb.Relationship> predicate)
      internal RelationshipFilter<T1>(System.Predicate<T1> predicate )
			  {
					this.Predicate = predicate;
			  }

   public override string ToString()
   {
      return Predicate.ToString();
   }

   internal override bool Exclude(IPath path)
   {
      return !Predicate.test(path.LastRelationship);
   }
}

private sealed class PathFilter : Filter
{
   //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
   //ORIGINAL LINE: private final System.Predicate<? super org.Neo4Net.graphdb.Path> predicate;
   //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
   internal readonly System.Predicate<object> Predicate;

   //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
   //ORIGINAL LINE: PathFilter(System.Predicate<? super org.Neo4Net.graphdb.Path> predicate)
   internal PathFilter<T1>(System.Predicate<T1> predicate )
			  {
					this.Predicate = predicate;
			  }

public override string ToString()
{
   return Predicate.ToString();
}

internal override bool Exclude(Path path)
{
   return !Predicate.test(path);
}
		 }

		 public StandardExpansion<Relationship> Expand(Node node)
{
   return new RelationshipExpansion(this, singleNodePath(node), BranchState.NO_STATE);
}

public override StandardExpansion<Relationship> Expand(Path path, BranchState state)
{
   return new RelationshipExpansion(this, path, state);
}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static <T> T[] append(T[] array, T item)
internal static T[] Append<T>(T[] array, T item)
{
   T[] result = (T[])Array.CreateInstance(array.GetType().GetElementType(), array.Length + 1);
   Array.Copy(array, 0, result, 0, array.Length);
   result[array.Length] = item;
   return result;
}

internal static bool MatchDirection(Direction dir, Node start, Relationship rel)
{
   switch (dir.innerEnumValue)
   {
      case Direction.InnerEnum.INCOMING:
         return rel.EndNode.Equals(start);

      case Direction.InnerEnum.OUTGOING:
         return rel.StartNode.Equals(start);

      case Direction.InnerEnum.BOTH:
         return true;

      default:
         throw new System.ArgumentException("Unknown direction: " + dir);
   }
}

internal abstract ResourceIterator<Relationship> DoExpand(Path path, BranchState state);

public override sealed string ToString()
{
   StringBuilder result = new StringBuilder("Expander[");
   BuildString(result);
   result.Append("]");
   return result.ToString();
}

internal abstract void BuildString(StringBuilder result);

public StandardExpander Add(RelationshipType type)
{
   return Add(type, Direction.BOTH);
}

public abstract StandardExpander Add(RelationshipType type, Direction direction);

public abstract StandardExpander Remove(RelationshipType type);

public override abstract StandardExpander Reverse();

public abstract StandardExpander Reversed();

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public StandardExpander addNodeFilter(System.Predicate<? super org.Neo4Net.graphdb.Node> filter)
public virtual StandardExpander AddNodeFilter<T1>(System.Predicate<T1> filter)
{
   return new FilteringExpander(this, new NodeFilter(filter));
}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public StandardExpander addRelationshipFilter(System.Predicate<? super org.Neo4Net.graphdb.Relationship> filter)
public virtual StandardExpander AddRelationshipFilter<T1>(System.Predicate<T1> filter)
{
   return new FilteringExpander(this, new RelationshipFilter(filter));
}

internal static StandardExpander Wrap(PathExpander expander)
{
   return new WrappingExpander(expander);
}

public static StandardExpander Create(Direction direction)
{
   return new AllExpander(direction);
}

public static StandardExpander Create(RelationshipType type, Direction dir)
{
   IDictionary<Direction, RelationshipType[]> types = new Dictionary<Direction, RelationshipType[]>(typeof(Direction));
   types[dir] = new RelationshipType[] { type };
   return new RegularExpander(types);
}

internal static StandardExpander Create(RelationshipType type1, Direction dir1, RelationshipType type2, Direction dir2)
{
   IDictionary<Direction, ICollection<RelationshipType>> tempMap = TemporaryTypeMap();
   tempMap[dir1].Add(type1);
   tempMap[dir2].Add(type2);
   return new RegularExpander(ToTypeMap(tempMap));
}

private static IDictionary<Direction, RelationshipType[]> ToTypeMap(IDictionary<Direction, ICollection<RelationshipType>> tempMap)
{
   // Remove OUT/IN where there is a BOTH
   ICollection<RelationshipType> both = tempMap[Direction.BOTH];
   //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
   tempMap[Direction.OUTGOING].removeAll(both);
   //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
   tempMap[Direction.INCOMING].removeAll(both);

   // Convert into a final map
   IDictionary<Direction, RelationshipType[]> map = new Dictionary<Direction, RelationshipType[]>(typeof(Direction));
   foreach (KeyValuePair<Direction, ICollection<RelationshipType>> entry in tempMap.SetOfKeyValuePairs())
   {
      if (!entry.Value.Empty)
      {
         map[entry.Key] = entry.Value.toArray(new RelationshipType[entry.Value.size()]);
      }
   }
   return map;
}

private static IDictionary<Direction, ICollection<RelationshipType>> TemporaryTypeMap()
{
   IDictionary<Direction, ICollection<RelationshipType>> map = new Dictionary<Direction, ICollection<RelationshipType>>(typeof(Direction));
   foreach (Direction direction in Direction.values())
   {
      map[direction] = new List<RelationshipType>();
   }
   return map;
}

private static IDictionary<Direction, ICollection<RelationshipType>> TemporaryTypeMapFrom(IDictionary<Direction, RelationshipType[]> typeMap)
{
   IDictionary<Direction, ICollection<RelationshipType>> map = new Dictionary<Direction, ICollection<RelationshipType>>(typeof(Direction));
   foreach (Direction direction in Direction.values())
   {
      List<RelationshipType> types = new List<RelationshipType>();
      map[direction] = types;
      RelationshipType[] existing = typeMap[direction];
      if (existing != null)
      {
         types.AddRange(asList(existing));
      }
   }
   return map;
}

public static StandardExpander Create(RelationshipType type1, Direction dir1, RelationshipType type2, Direction dir2, params object[] more)
{
   IDictionary<Direction, ICollection<RelationshipType>> tempMap = TemporaryTypeMap();
   tempMap[dir1].Add(type1);
   tempMap[dir2].Add(type2);
   for (int i = 0; i < more.Length; i++)
   {
      RelationshipType type = (RelationshipType)more[i++];
      Direction direction = (Direction)more[i];
      tempMap[direction].Add(type);
   }
   return new RegularExpander(ToTypeMap(tempMap));
}
	}
}