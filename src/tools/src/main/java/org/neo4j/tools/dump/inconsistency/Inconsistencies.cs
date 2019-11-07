/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.tools.dump.inconsistency
{
	using RecordType = Neo4Net.Consistency.RecordType;

	/// <summary>
	/// Container for ids of entities that are considered to be inconsistent.
	/// </summary>
	public interface IInconsistencies
	{
		 void Node( long id );

		 void Relationship( long id );

		 void Property( long id );

		 void RelationshipGroup( long id );

		 void SchemaIndex( long id );

		 bool ContainsNodeId( long id );

		 bool ContainsRelationshipId( long id );

		 bool ContainsPropertyId( long id );

		 bool ContainsRelationshipGroupId( long id );

		 bool ContainsSchemaIndexId( long id );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void reportInconsistency(Neo4Net.consistency.RecordType recordType, long recordId)
	//	 {
	//		  if (recordType == null)
	//		  {
	//				// Skip records of unknown type.
	//				return;
	//		  }
	//
	//		  switch (recordType)
	//		  {
	//		  case NODE:
	//				node(recordId);
	//				break;
	//		  case RELATIONSHIP:
	//				relationship(recordId);
	//				break;
	//		  case PROPERTY:
	//				property(recordId);
	//				break;
	//		  case RELATIONSHIP_GROUP:
	//				relationshipGroup(recordId);
	//				break;
	//		  case SCHEMA:
	//				schemaIndex(recordId);
	//				break;
	//		  default:
	//				// Ignore unknown record types.
	//				break;
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void reportInconsistency(Neo4Net.consistency.RecordType recordType, long recordId, Neo4Net.consistency.RecordType inconsistentWithRecordType, long inconsistentWithRecordId)
	//	 {
	//		  reportInconsistency(recordType, recordId);
	//		  reportInconsistency(inconsistentWithRecordType, inconsistentWithRecordId);
	//	 }
	}

}