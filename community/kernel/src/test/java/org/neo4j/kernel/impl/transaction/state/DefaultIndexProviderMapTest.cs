﻿/*
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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using Test = org.junit.jupiter.api.Test;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexProviderNotFoundException = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderNotFoundException;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class DefaultIndexProviderMapTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotSupportMultipleProvidersWithSameDescriptor()
		 internal virtual void ShouldNotSupportMultipleProvidersWithSameDescriptor()
		 {
			  // given
			  IndexProviderDescriptor descriptor = new IndexProviderDescriptor( "provider", "1.2" );
			  IndexProvider provider1 = mock( typeof( IndexProvider ) );
			  when( provider1.ProviderDescriptor ).thenReturn( descriptor );
			  IndexProvider provider2 = mock( typeof( IndexProvider ) );
			  when( provider2.ProviderDescriptor ).thenReturn( descriptor );

			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( provider1 );
			  dependencies.SatisfyDependency( provider2 );

			  // when
			  assertThrows( typeof( System.ArgumentException ), () => CreateDefaultProviderMap(dependencies, descriptor).init() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnLookupOnUnknownProvider()
		 internal virtual void ShouldThrowOnLookupOnUnknownProvider()
		 {
			  // given
			  IndexProvider provider = mock( typeof( IndexProvider ) );
			  IndexProviderDescriptor descriptor = new IndexProviderDescriptor( "provider", "1.2" );
			  when( provider.ProviderDescriptor ).thenReturn( descriptor );
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( provider );

			  // when
			  DefaultIndexProviderMap defaultIndexProviderMap = CreateDefaultProviderMap( dependencies, descriptor );
			  defaultIndexProviderMap.Init();
			  assertThrows( typeof( IndexProviderNotFoundException ), () => defaultIndexProviderMap.Lookup(new IndexProviderDescriptor("provider2", "1.2")) );
		 }

		 private static DefaultIndexProviderMap CreateDefaultProviderMap( Dependencies dependencies, IndexProviderDescriptor descriptor )
		 {
			  return new DefaultIndexProviderMap( dependencies, Config.defaults( GraphDatabaseSettings.default_schema_provider, descriptor.Name() ) );
		 }
	}

}