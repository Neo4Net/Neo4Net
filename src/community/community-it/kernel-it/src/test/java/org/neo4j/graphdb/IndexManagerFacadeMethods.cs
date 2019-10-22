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
namespace Neo4Net.GraphDb
{

	using IndexManager = Neo4Net.GraphDb.index.IndexManager;

	public sealed class IndexManagerFacadeMethods : Consumer<IndexManager>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       EXISTS_FOR_NODES(new FacadeMethod<>("boolean existsForNodes( String indexName )", self -> self.existsForNodes("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FOR_NODES(new FacadeMethod<>("Index<Node> forNodes( String indexName )", self -> self.forNodes("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FOR_NODES_WITH_CONFIG(new FacadeMethod<>("Index<Node> forNodes( String indexName, Map<String, String> customConfiguration )", self -> self.forNodes("foo", null))),
		 public static readonly IndexManagerFacadeMethods NodeIndexNames = new IndexManagerFacadeMethods( "NodeIndexNames", InnerEnum.NodeIndexNames, new FacadeMethod<>( "String[] nodeIndexNames()", Neo4Net.GraphDb.index.IndexManager::nodeIndexNames ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       EXISTS_FOR_RELATIONSHIPS(new FacadeMethod<>("boolean existsForRelationships( String indexName )", self -> self.existsForRelationships("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FOR_RELATIONSHIPS(new FacadeMethod<>("RelationshipIndex forRelationships( String indexName )", self -> self.forRelationships("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FOR_RELATIONSHIPS_WITH_CONFIG(new FacadeMethod<>("RelationshipIndex forRelationships( String indexName, Map<String, String> customConfiguration )", self -> self.forRelationships("foo", null))),
		 public static readonly IndexManagerFacadeMethods RelationshipIndexNames = new IndexManagerFacadeMethods( "RelationshipIndexNames", InnerEnum.RelationshipIndexNames, new FacadeMethod<>( "String[] relationshipIndexNames()", Neo4Net.GraphDb.index.IndexManager::relationshipIndexNames ) );

		 private static readonly IList<IndexManagerFacadeMethods> valueList = new List<IndexManagerFacadeMethods>();

		 static IndexManagerFacadeMethods()
		 {
			 valueList.Add( EXISTS_FOR_NODES );
			 valueList.Add( FOR_NODES );
			 valueList.Add( FOR_NODES_WITH_CONFIG );
			 valueList.Add( NodeIndexNames );
			 valueList.Add( EXISTS_FOR_RELATIONSHIPS );
			 valueList.Add( FOR_RELATIONSHIPS );
			 valueList.Add( FOR_RELATIONSHIPS_WITH_CONFIG );
			 valueList.Add( RelationshipIndexNames );
		 }

		 public enum InnerEnum
		 {
			 EXISTS_FOR_NODES,
			 FOR_NODES,
			 FOR_NODES_WITH_CONFIG,
			 NodeIndexNames,
			 EXISTS_FOR_RELATIONSHIPS,
			 FOR_RELATIONSHIPS,
			 FOR_RELATIONSHIPS_WITH_CONFIG,
			 RelationshipIndexNames
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal IndexManagerFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Neo4Net.GraphDb.index.IndexManager> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Neo4Net.GraphDb.index.IndexManager indexManager )
		 {
			  _facadeMethod.accept( indexManager );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<IndexManagerFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static IndexManagerFacadeMethods valueOf( string name )
		{
			foreach ( IndexManagerFacadeMethods enumInstance in IndexManagerFacadeMethods.valueList )
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