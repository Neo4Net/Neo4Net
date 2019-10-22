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
namespace Neo4Net.Consistency.checking.full
{
	using Neo4Net.Consistency.report;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.store.RecordReference_SkippingReference.skipReference;

	internal abstract class PropertyOwner<RECORD> : Owner where RECORD : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord
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