using System;

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
namespace Neo4Net.Kernel.Api.Exceptions.index
{
	using Test = org.junit.Test;

	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;
	using SchemaUtil = Neo4Net.@internal.Kernel.Api.schema.SchemaUtil;
	using LabelSchemaDescriptor = Neo4Net.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;

	public class IndexPopulationFailedKernelExceptionTest
	{

		 private static readonly TokenNameLookup _tokenNameLookup = SchemaUtil.idTokenNameLookup;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultiplePropertiesInConstructor1()
		 public virtual void ShouldHandleMultiplePropertiesInConstructor1()
		 {
			  // Given
			  LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( 0, 42, 43, 44 );

			  // When
			  IndexPopulationFailedKernelException index = new IndexPopulationFailedKernelException( descriptor.UserDescription( _tokenNameLookup ), new Exception() );

			  // Then
			  assertThat( index.GetUserMessage( _tokenNameLookup ), equalTo( "Failed to populate index :label[0](property[42], property[43], property[44])" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultiplePropertiesInConstructor2()
		 public virtual void ShouldHandleMultiplePropertiesInConstructor2()
		 {
			  // Given
			  LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( 0, 42, 43, 44 );

			  // When
			  IndexPopulationFailedKernelException index = new IndexPopulationFailedKernelException( descriptor.UserDescription( _tokenNameLookup ), "an act of pure evil occurred" );

			  // Then
			  assertThat( index.GetUserMessage( _tokenNameLookup ), equalTo( "Failed to populate index :label[0](property[42], property[43], property[44]), due to an act of pure evil occurred" ) );
		 }
	}

}