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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Neo4Net.Kernel.Api.Internal;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;

	public class ConstraintTest : ConstraintTestBase<WriteTestSupport>
	{
		 public override WriteTestSupport NewTestSupport()
		 {
			  return new WriteTestSupport();
		 }

		 protected internal override LabelSchemaDescriptor LabelSchemaDescriptor( int labelId, params int[] propertyIds )
		 {
			  return SchemaDescriptorFactory.forLabel( labelId, propertyIds );
		 }

		 protected internal override ConstraintDescriptor UniqueConstraintDescriptor( int labelId, params int[] propertyIds )
		 {
			  return ConstraintDescriptorFactory.uniqueForLabel( labelId, propertyIds );
		 }
	}

}