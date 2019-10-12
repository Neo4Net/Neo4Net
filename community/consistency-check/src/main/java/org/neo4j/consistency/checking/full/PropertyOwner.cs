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
namespace Org.Neo4j.Consistency.checking.full
{
	using Org.Neo4j.Consistency.report;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using Org.Neo4j.Consistency.store;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference;

	internal abstract class PropertyOwner<RECORD> : Owner where RECORD : Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord
	{
		 internal abstract RecordReference<RECORD> Record( RecordAccess records );

		 public override void CheckOrphanage()
		 {
			  // default: do nothing
		 }

		 internal class OwningNode : PropertyOwner<NodeRecord>
		 {
			  internal readonly long Id;

			  internal OwningNode( NodeRecord record )
			  {
					this.Id = record.Id;
			  }

			  internal override RecordReference<NodeRecord> Record( RecordAccess records )
			  {
					return records.Node( Id );
			  }
		 }

		 internal class OwningRelationship : PropertyOwner<RelationshipRecord>
		 {
			  internal readonly long Id;

			  internal OwningRelationship( RelationshipRecord record )
			  {
					this.Id = record.Id;
			  }

			  internal override RecordReference<RelationshipRecord> Record( RecordAccess records )
			  {
					return records.Relationship( Id );
			  }
		 }

		 internal static readonly PropertyOwner<NeoStoreRecord> OWNING_GRAPH = new PropertyOwnerAnonymousInnerClass();

		 private class PropertyOwnerAnonymousInnerClass : PropertyOwner<NeoStoreRecord>
		 {
			 internal override RecordReference<NeoStoreRecord> record( RecordAccess records )
			 {
				  return records.Graph();
			 }
		 }

		 internal class UnknownOwner : PropertyOwner<PrimitiveRecord>, RecordReference<PrimitiveRecord>
		 {
			  internal PendingReferenceCheck<PrimitiveRecord> Reporter;

			  internal override RecordReference<PrimitiveRecord> Record( RecordAccess records )
			  {
					// Getting the record for this owner means that some other owner replaced it
					// that means that it isn't an orphan, so we skip this orphan check
					// and return a record for conflict check that always is ok (by skipping the check)
					this.MarkInCustody();
					return skipReference();
			  }

			  public override void CheckOrphanage()
			  {
					PendingReferenceCheck<PrimitiveRecord> reporter;
					lock ( this )
					{
						 reporter = this.Reporter;
						 this.Reporter = null;
					}
					if ( reporter != null )
					{
						 reporter.CheckReference( null, null );
					}
			  }

			  internal virtual void MarkInCustody()
			  {
				  lock ( this )
				  {
						if ( Reporter != null )
						{
							 Reporter.skip();
							 Reporter = null;
						}
				  }
			  }

			  public override void Dispatch( PendingReferenceCheck<PrimitiveRecord> reporter )
			  {
				  lock ( this )
				  {
						this.Reporter = reporter;
				  }
			  }
		 }

		 private PropertyOwner()
		 {
			  // only internal subclasses
		 }
	}

}