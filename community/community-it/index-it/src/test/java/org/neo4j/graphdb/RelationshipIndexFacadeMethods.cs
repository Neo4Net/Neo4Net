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

	using RelationshipIndex = Org.Neo4j.Graphdb.index.RelationshipIndex;

	public sealed class RelationshipIndexFacadeMethods : Consumer<RelationshipIndex>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_WITH_FILTER(new FacadeMethod<>("IndexHits<Relationship> get( String key, Object valueOrNull, Node startNodeOrNull, Node endNodeOrNull )", ri -> ri.get("foo", 42, null, null))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       QUERY_BY_KEY_WITH_FILTER(new FacadeMethod<>("IndexHits<Relationship> query( String key, Object queryOrQueryObjectOrNull, Node startNodeOrNull, Node endNodeOrNull )", ri -> ri.query("foo", 42, null, null))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       QUERY_WITH_FILTER(new FacadeMethod<>("IndexHits<Relationship> query( Object queryOrQueryObjectOrNull, Node startNodeOrNull, Node endNodeOrNull )", ri -> ri.query(42, null, null))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET(new FacadeMethod<>("IndexHits<T> get( String key, Object value )", ri -> ri.get("foo", "bar"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       QUERY_BY_KEY(new FacadeMethod<>("IndexHits<T> query( String key, Object queryOrQueryObject )", ri -> ri.query("foo", "bar"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       QUERY(new FacadeMethod<>("IndexHits<T> query( Object queryOrQueryObject )", ri -> ri.query("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ADD(new FacadeMethod<>("void add( T entity, String key, Object value )", ri -> ri.add(null, "foo", 42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE_BY_KEY_AND_VALUE(new FacadeMethod<>("void remove( T entity, String key, Object value )", ri -> ri.remove(null, "foo", 42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE_BY_KEY(new FacadeMethod<>("void remove( T entity, String key )", ri -> ri.remove(null, "foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE(new FacadeMethod<>("void remove( T entity )", ri -> ri.remove(null))),
		 public static readonly RelationshipIndexFacadeMethods Delete = new RelationshipIndexFacadeMethods( "Delete", InnerEnum.Delete, new FacadeMethod<>( "void delete()", Org.Neo4j.Graphdb.index.RelationshipIndex.delete ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PUT_IF_ABSENT(new FacadeMethod<>("T putIfAbsent( T entity, String key, Object value )", ri -> ri.putIfAbsent(null, "foo", 42)));

		 private static readonly IList<RelationshipIndexFacadeMethods> valueList = new List<RelationshipIndexFacadeMethods>();

		 static RelationshipIndexFacadeMethods()
		 {
			 valueList.Add( GET_WITH_FILTER );
			 valueList.Add( QUERY_BY_KEY_WITH_FILTER );
			 valueList.Add( QUERY_WITH_FILTER );
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
			 GET_WITH_FILTER,
			 QUERY_BY_KEY_WITH_FILTER,
			 QUERY_WITH_FILTER,
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

		 private readonly FacadeMethod<Org.Neo4j.Graphdb.index.RelationshipIndex> facadeMethod;

		 internal RelationshipIndexFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Org.Neo4j.Graphdb.index.RelationshipIndex> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Org.Neo4j.Graphdb.index.RelationshipIndex relationshipIndex )
		 {
			  _facadeMethod.accept( relationshipIndex );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<RelationshipIndexFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static RelationshipIndexFacadeMethods valueOf( string name )
		{
			foreach ( RelationshipIndexFacadeMethods enumInstance in RelationshipIndexFacadeMethods.valueList )
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