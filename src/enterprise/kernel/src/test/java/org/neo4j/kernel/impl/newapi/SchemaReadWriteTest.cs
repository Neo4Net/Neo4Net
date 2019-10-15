﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Neo4Net.Internal.Kernel.Api;
	using LabelSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forRelType;

	public class SchemaReadWriteTest : SchemaReadWriteTestBase<EnterpriseWriteTestSupport>
	{
		 public override EnterpriseWriteTestSupport NewTestSupport()
		 {
			  return new EnterpriseWriteTestSupport();
		 }

		 protected internal override LabelSchemaDescriptor LabelDescriptor( int label, params int[] props )
		 {
			  return SchemaDescriptorFactory.forLabel( label, props );
		 }

		 protected internal override RelationTypeSchemaDescriptor TypeDescriptor( int label, params int[] props )
		 {
			  return forRelType( label, props );
		 }
	}

}