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
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	/// <summary>
	/// IEntity ids that reported to be inconsistent in consistency report where they where extracted from.
	/// </summary>
	public class ReportInconsistencies : Inconsistencies
	{
		 private readonly MutableLongSet _schemaIndexesIds = new LongHashSet();
		 private readonly MutableLongSet _relationshipIds = new LongHashSet();
		 private readonly MutableLongSet _nodeIds = new LongHashSet();
		 private readonly MutableLongSet _propertyIds = new LongHashSet();
		 private readonly MutableLongSet _relationshipGroupIds = new LongHashSet();

		 public override void RelationshipGroup( long id )
		 {
			  _relationshipGroupIds.add( id );
		 }

		 public override void SchemaIndex( long id )
		 {
			  _schemaIndexesIds.add( id );
		 }

		 public override void Relationship( long id )
		 {
			  _relationshipIds.add( id );
		 }

		 public override void Property( long id )
		 {
			  _propertyIds.add( id );
		 }

		 public override void Node( long id )
		 {
			  _nodeIds.add( id );
		 }

		 public override bool ContainsNodeId( long id )
		 {
			  return _nodeIds.contains( id );
		 }

		 public override bool ContainsRelationshipId( long id )
		 {
			  return _relationshipIds.contains( id );
		 }

		 public override bool ContainsPropertyId( long id )
		 {
			  return _propertyIds.contains( id );
		 }

		 public override bool ContainsRelationshipGroupId( long id )
		 {
			  return _relationshipGroupIds.contains( id );
		 }

		 public override bool ContainsSchemaIndexId( long id )
		 {
			  return _schemaIndexesIds.contains( id );
		 }
	}

}