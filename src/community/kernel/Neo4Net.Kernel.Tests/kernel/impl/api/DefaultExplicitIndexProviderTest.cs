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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.jupiter.api.Test;

	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	internal class DefaultExplicitIndexProviderTest
	{
		 private readonly DefaultExplicitIndexProvider _provider = new DefaultExplicitIndexProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void registerAndAccessIndexProvider()
		 internal virtual void RegisterAndAccessIndexProvider()
		 {
			  IndexImplementation index = mock( typeof( IndexImplementation ) );
			  string testProviderName = "a";
			  _provider.registerIndexProvider( testProviderName, index );
			  assertSame( index, _provider.getProviderByName( testProviderName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwOnAttemptToRegisterProviderWithSameName()
		 internal virtual void ThrowOnAttemptToRegisterProviderWithSameName()
		 {
			  IndexImplementation index = mock( typeof( IndexImplementation ) );
			  string testProviderName = "a";
			  _provider.registerIndexProvider( testProviderName, index );
			  assertThrows( typeof( System.ArgumentException ), () => _provider.registerIndexProvider(testProviderName, index) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void unregisterIndexProvider()
		 internal virtual void UnregisterIndexProvider()
		 {
			  IndexImplementation index = mock( typeof( IndexImplementation ) );
			  string testProviderName = "b";
			  _provider.registerIndexProvider( testProviderName, index );
			  assertTrue( _provider.unregisterIndexProvider( testProviderName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removeNotExistentProvider()
		 internal virtual void RemoveNotExistentProvider()
		 {
			  assertFalse( _provider.unregisterIndexProvider( "c" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwOnAttemptToGetNonRegisteredProviderByName()
		 internal virtual void ThrowOnAttemptToGetNonRegisteredProviderByName()
		 {
			  string testProviderName = "d";
			  assertThrows( typeof( System.ArgumentException ), () => _provider.getProviderByName(testProviderName) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void accessAllRegisteredIndexProviders()
		 internal virtual void AccessAllRegisteredIndexProviders()
		 {
			  IndexImplementation index1 = mock( typeof( IndexImplementation ) );
			  IndexImplementation index2 = mock( typeof( IndexImplementation ) );
			  string testProviderName1 = "e";
			  string testProviderName2 = "f";
			  _provider.registerIndexProvider( testProviderName1, index1 );
			  _provider.registerIndexProvider( testProviderName2, index2 );

			  assertThat( _provider.allIndexProviders(), contains(index1, index2) );
		 }
	}

}