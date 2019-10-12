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
namespace Org.Neo4j.Consistency.store
{

	using MultiPassStore = Org.Neo4j.Consistency.checking.full.MultiPassStore;
	using Org.Neo4j.Helpers.Collection;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference;

	public class FilteringRecordAccess : DelegatingRecordAccess
	{
		 private readonly ISet<MultiPassStore> _potentiallySkippableStores = EnumSet.noneOf( typeof( MultiPassStore ) );
		 private readonly MultiPassStore _currentStore;

		 public FilteringRecordAccess( RecordAccess @delegate, MultiPassStore currentStore, params MultiPassStore[] potentiallySkippableStores ) : base( @delegate )
		 {
			  this._currentStore = currentStore;
			  this._potentiallySkippableStores.addAll( asList( potentiallySkippableStores ) );
		 }

		 internal enum Mode
		 {
			  Skip,
			  Filter
		 }

		 public override RecordReference<NodeRecord> Node( long id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.NODES ) )
			  {
					return base.Node( id );
			  }
			  return skipReference();
		 }

		 public override RecordReference<RelationshipRecord> Relationship( long id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.RELATIONSHIPS ) )
			  {
					return base.Relationship( id );
			  }
			  return skipReference();
		 }

		 public override RecordReference<RelationshipGroupRecord> RelationshipGroup( long id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.RELATIONSHIP_GROUPS ) )
			  {
					return base.RelationshipGroup( id );
			  }
			  return skipReference();
		 }

		 public override RecordReference<PropertyRecord> Property( long id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.PROPERTIES ) )
			  {
					return base.Property( id );
			  }
			  return skipReference();
		 }

		 public override IEnumerator<PropertyRecord> RawPropertyChain( long firstId )
		 {
			  return new FilteringIterator<PropertyRecord>( base.RawPropertyChain( firstId ), item => ShouldCheck( item.Id, MultiPassStore.PROPERTIES ) );
		 }

		 public override RecordReference<PropertyKeyTokenRecord> PropertyKey( int id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.PROPERTY_KEYS ) )
			  {
					return base.PropertyKey( id );
			  }
			  return skipReference();
		 }

		 public override RecordReference<DynamicRecord> String( long id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.STRINGS ) )
			  {
					return base.String( id );
			  }
			  return skipReference();
		 }

		 public override RecordReference<DynamicRecord> Array( long id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.ARRAYS ) )
			  {
					return base.Array( id );
			  }
			  return skipReference();
		 }

		 public override RecordReference<LabelTokenRecord> Label( int id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.LABELS ) )
			  {
					return base.Label( id );
			  }
			  return skipReference();
		 }

		 public override RecordReference<DynamicRecord> NodeLabels( long id )
		 {
			  if ( ShouldCheck( id, MultiPassStore.LABELS ) )
			  {
					return base.NodeLabels( id );
			  }
			  return skipReference();
		 }

		 public override bool ShouldCheck( long id, MultiPassStore store )
		 {
			  return !( _potentiallySkippableStores.Contains( store ) && ( _currentStore != store ) );
		 }
	}

}