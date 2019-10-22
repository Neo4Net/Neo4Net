﻿/*
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
namespace Neo4Net.Consistency.checking
{
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;

	public class SchemaRuleUtil
	{
		 private SchemaRuleUtil()
		 {
		 }

		 public static ConstraintRule UniquenessConstraintRule( long ruleId, int labelId, int propertyId, long indexId )
		 {
			  return ConstraintRule.constraintRule( ruleId, ConstraintDescriptorFactory.uniqueForLabel( labelId, propertyId ), indexId );
		 }

		 public static ConstraintRule NodePropertyExistenceConstraintRule( long ruleId, int labelId, int propertyId )
		 {
			  return ConstraintRule.constraintRule( ruleId, ConstraintDescriptorFactory.existsForLabel( labelId, propertyId ) );
		 }

		 public static ConstraintRule RelPropertyExistenceConstraintRule( long ruleId, int labelId, int propertyId )
		 {
			  return ConstraintRule.constraintRule( ruleId, ConstraintDescriptorFactory.existsForRelType( labelId, propertyId ) );
		 }

		 public static StoreIndexDescriptor IndexRule( long ruleId, int labelId, int propertyId, IndexProviderDescriptor descriptor )
		 {
			  return IndexDescriptorFactory.forSchema( forLabel( labelId, propertyId ), descriptor ).withId( ruleId );
		 }

		 public static StoreIndexDescriptor ConstraintIndexRule( long ruleId, int labelId, int propertyId, IndexProviderDescriptor descriptor, long constraintId )
		 {
			  return IndexDescriptorFactory.uniqueForSchema( forLabel( labelId, propertyId ), descriptor ).withIds( ruleId, constraintId );
		 }

		 public static StoreIndexDescriptor ConstraintIndexRule( long ruleId, int labelId, int propertyId, IndexProviderDescriptor descriptor )
		 {
			  return IndexDescriptorFactory.uniqueForSchema( forLabel( labelId, propertyId ), descriptor ).withId( ruleId );
		 }
	}

}