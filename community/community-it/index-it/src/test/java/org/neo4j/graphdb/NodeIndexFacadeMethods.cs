using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Graphdb
{

	using Org.Neo4j.Graphdb.index;

	public sealed class NodeIndexFacadeMethods : Consumer<Index<Node>>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET(new FacadeMethod<>("IndexHits<T> get( String key, Object value )", self -> self.get("foo", "bar"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       QUERY_BY_KEY(new FacadeMethod<>("IndexHits<T> query( String key, Object queryOrQueryObject )", self -> self.query("foo", "bar"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       QUERY(new FacadeMethod<>("IndexHits<T> query( Object queryOrQueryObject )", self -> self.query("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ADD(new FacadeMethod<>("void add( T entity, String key, Object value )", self -> self.add(null, "foo", 42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE_BY_KEY_AND_VALUE(new FacadeMethod<>("void remove( T entity, String key, Object value )", self -> self.remove(null, "foo", 42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE_BY_KEY(new FacadeMethod<>("void remove( T entity, String key )", self -> self.remove(null, "foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE(new FacadeMethod<>("void remove( T entity )", self -> self.remove(null))),
		 public static readonly NodeIndexFacadeMethods Delete = new NodeIndexFacadeMethods( "Delete", InnerEnum.Delete, new FacadeMethod<>( "void delete()", Org.Neo4j.Graphdb.index.Index.delete ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PUT_IF_ABSENT(new FacadeMethod<>("T putIfAbsent( T entity, String key, Object value )", self -> self.putIfAbsent(null, "foo", 42)));

		 private static readonly IList<NodeIndexFacadeMethods> valueList = new List<NodeIndexFacadeMethods>();

		 static NodeIndexFacadeMethods()
		 {
			 valueList.Add( GET );
			 valueList.Add( QUERY_BY_KEY );
			 valueList.Add( QUERY );
			 valueList.Add( ADD );
			 valueList.Add( REMOVE_BY_KEY_AND_VALUE );
			 valueList.Add( REMOVE_BY_KEY );
			 valueList.Add( REMOVE );
			 valueList.Add( Delete );
			 valueList.Add( PUT_IF_ABSENT );
		 }

		 public enum InnerEnum
		 {
			 GET,
			 QUERY_BY_KEY,
			 QUERY,
			 ADD,
			 REMOVE_BY_KEY_AND_VALUE,
			 REMOVE_BY_KEY,
			 REMOVE,
			 Delete,
			 PUT_IF_ABSENT
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private readonly FacadeMethod<Org.Neo4j.Graphdb.index.Index<Node>> facadeMethod;

		 internal NodeIndexFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Org.Neo4j.Graphdb.index.Index<Node>> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Org.Neo4j.Graphdb.index.Index<Node> nodeIndex )
		 {
			  _facadeMethod.accept( nodeIndex );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<NodeIndexFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static NodeIndexFacadeMethods valueOf( string name )
		{
			foreach ( NodeIndexFacadeMethods enumInstance in NodeIndexFacadeMethods.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}