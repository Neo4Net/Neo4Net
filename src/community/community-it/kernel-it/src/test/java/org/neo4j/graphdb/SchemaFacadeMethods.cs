using System.Collections.Generic;

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
namespace Neo4Net.Graphdb
{

	using Schema = Neo4Net.Graphdb.schema.Schema;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.FacadeMethod.INDEX_DEFINITION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.FacadeMethod.LABEL;

	public sealed class SchemaFacadeMethods : Consumer<Schema>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       INDEX_FOR(new FacadeMethod<>("IndexCreator indexFor( Label label )", s -> s.indexFor(LABEL))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_INDEXES_BY_LABEL(new FacadeMethod<>("Iterable<IndexDefinition> getIndexes( Label label )", s -> s.getIndexes(LABEL))),
		 public static readonly SchemaFacadeMethods GetIndexes = new SchemaFacadeMethods( "GetIndexes", InnerEnum.GetIndexes, new FacadeMethod<>( "Iterable<IndexDefinition> getIndexes()", Neo4Net.Graphdb.schema.Schema::getIndexes ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_INDEX_STATE(new FacadeMethod<>("IndexState getIndexState( IndexDefinition index )", s -> s.getIndexState(INDEX_DEFINITION))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_INDEX_FAILURE(new FacadeMethod<>("String getIndexFailure( IndexDefinition index )", s -> s.getIndexFailure(INDEX_DEFINITION))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CONSTRAINT_FOR(new FacadeMethod<>("ConstraintCreator constraintFor( Label label )", s -> s.constraintFor(LABEL))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_CONSTRAINTS_BY_LABEL(new FacadeMethod<>("Iterable<ConstraintDefinition> getConstraints( Label label )", s -> s.getConstraints(LABEL))),
		 public static readonly SchemaFacadeMethods GetConstraints = new SchemaFacadeMethods( "GetConstraints", InnerEnum.GetConstraints, new FacadeMethod<>( "Iterable<ConstraintDefinition> getConstraints()", Neo4Net.Graphdb.schema.Schema::getConstraints ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       AWAIT_INDEX_ONLINE(new FacadeMethod<>("void awaitIndexOnline( IndexDefinition index, long duration, TimeUnit unit )", s -> s.awaitIndexOnline(INDEX_DEFINITION, 1L, java.util.concurrent.TimeUnit.SECONDS))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       AWAIT_INDEXES_ONLINE(new FacadeMethod<>("void awaitIndexesOnline( long duration, TimeUnit unit )", s -> s.awaitIndexesOnline(1L, java.util.concurrent.TimeUnit.SECONDS)));

		 private static readonly IList<SchemaFacadeMethods> valueList = new List<SchemaFacadeMethods>();

		 static SchemaFacadeMethods()
		 {
			 valueList.Add( INDEX_FOR );
			 valueList.Add( GET_INDEXES_BY_LABEL );
			 valueList.Add( GetIndexes );
			 valueList.Add( GET_INDEX_STATE );
			 valueList.Add( GET_INDEX_FAILURE );
			 valueList.Add( CONSTRAINT_FOR );
			 valueList.Add( GET_CONSTRAINTS_BY_LABEL );
			 valueList.Add( GetConstraints );
			 valueList.Add( AWAIT_INDEX_ONLINE );
			 valueList.Add( AWAIT_INDEXES_ONLINE );
		 }

		 public enum InnerEnum
		 {
			 INDEX_FOR,
			 GET_INDEXES_BY_LABEL,
			 GetIndexes,
			 GET_INDEX_STATE,
			 GET_INDEX_FAILURE,
			 CONSTRAINT_FOR,
			 GET_CONSTRAINTS_BY_LABEL,
			 GetConstraints,
			 AWAIT_INDEX_ONLINE,
			 AWAIT_INDEXES_ONLINE
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private readonly FacadeMethod<Neo4Net.Graphdb.schema.Schema> facadeMethod;

		 internal SchemaFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Neo4Net.Graphdb.schema.Schema> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Neo4Net.Graphdb.schema.Schema schema )
		 {
			  _facadeMethod.accept( schema );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<SchemaFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static SchemaFacadeMethods valueOf( string name )
		{
			foreach ( SchemaFacadeMethods enumInstance in SchemaFacadeMethods.valueList )
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