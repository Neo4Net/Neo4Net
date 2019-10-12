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
namespace Org.Neo4j.Consistency.checking
{
	using ConsistencyReport_LabelTokenConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport;
	using ConsistencyReport_NeoStoreConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using ConsistencyReport_PropertyConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport;
	using ConsistencyReport_PropertyKeyTokenConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport;
	using ConsistencyReport_RelationshipConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using ConsistencyReport_RelationshipGroupConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport;
	using ConsistencyReport_RelationshipTypeConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

	public interface CheckDecorator
	{
		 /// <summary>
		 /// Called before each pass over the store(s) to check.
		 /// </summary>
		 void Prepare();

		 OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> DecorateNeoStoreChecker( OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> checker );

		 OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> DecorateNodeChecker( OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker );

		 OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> DecorateRelationshipChecker( OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker );

		 RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> DecoratePropertyChecker( RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker );

		 RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> DecoratePropertyKeyTokenChecker( RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker );

		 RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> DecorateRelationshipTypeTokenChecker( RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker );

		 RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> DecorateLabelTokenChecker( RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker );

		 RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> DecorateRelationshipGroupChecker( RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker );
	}

	public static class CheckDecorator_Fields
	{
		 public static readonly CheckDecorator None = new CheckDecorator_Adapter();
	}

	 public class CheckDecorator_Adapter : CheckDecorator
	 {
		  public override void Prepare()
		  {
		  }

		  public override OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> DecorateNeoStoreChecker( OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> checker )
		  {
				return checker;
		  }

		  public override OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> DecorateNodeChecker( OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker )
		  {
				return checker;
		  }

		  public override OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> DecorateRelationshipChecker( OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker )
		  {
				return checker;
		  }

		  public override RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> DecoratePropertyChecker( RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker )
		  {
				return checker;
		  }

		  public override RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> DecoratePropertyKeyTokenChecker( RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker )
		  {
				return checker;
		  }

		  public override RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> DecorateRelationshipTypeTokenChecker( RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker )
		  {
				return checker;
		  }

		  public override RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> DecorateLabelTokenChecker( RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker )
		  {
				return checker;
		  }

		  public override RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> DecorateRelationshipGroupChecker( RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker )
		  {
				return checker;
		  }
	 }

	 public class CheckDecorator_ChainCheckDecorator : CheckDecorator
	 {
		  internal readonly CheckDecorator[] Decorators;

		  public CheckDecorator_ChainCheckDecorator( params CheckDecorator[] decorators )
		  {
				this.Decorators = decorators;
		  }

		  public override void Prepare()
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 decorator.Prepare();
				}
		  }

		  public override OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> DecorateNeoStoreChecker( OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> checker )
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 checker = decorator.DecorateNeoStoreChecker( checker );
				}
				return checker;
		  }

		  public override OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> DecorateNodeChecker( OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker )
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 checker = decorator.DecorateNodeChecker( checker );
				}
				return checker;
		  }

		  public override OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> DecorateRelationshipChecker( OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker )
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 checker = decorator.DecorateRelationshipChecker( checker );
				}
				return checker;
		  }

		  public override RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> DecoratePropertyChecker( RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker )
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 checker = decorator.DecoratePropertyChecker( checker );
				}
				return checker;
		  }

		  public override RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> DecoratePropertyKeyTokenChecker( RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker )
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 checker = decorator.DecoratePropertyKeyTokenChecker( checker );
				}
				return checker;

		  }

		  public override RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> DecorateRelationshipTypeTokenChecker( RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker )
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 checker = decorator.DecorateRelationshipTypeTokenChecker( checker );
				}
				return checker;
		  }

		  public override RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> DecorateLabelTokenChecker( RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker )
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 checker = decorator.DecorateLabelTokenChecker( checker );
				}
				return checker;
		  }

		  public override RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> DecorateRelationshipGroupChecker( RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker )
		  {
				foreach ( CheckDecorator decorator in Decorators )
				{
					 checker = decorator.DecorateRelationshipGroupChecker( checker );
				}
				return checker;
		  }
	 }

}