using System;

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
namespace Neo4Net.Server.rest.discovery
{
	using Test = org.junit.Test;


	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.only;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.discovery.DiscoverableURIs.Precedence.HIGH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.discovery.DiscoverableURIs.Precedence.HIGHEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.discovery.DiscoverableURIs.Precedence.LOW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.discovery.DiscoverableURIs.Precedence.LOWEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.discovery.DiscoverableURIs.Precedence.NORMAL;

	public class DiscoverableURIsTest
	{
		 private System.Action<string, URI> _consumer = mock( typeof( System.Action ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotInvokeConsumerWhenEmpty()
		 public virtual void ShouldNotInvokeConsumerWhenEmpty()
		 {
			  DiscoverableURIs empty = ( new DiscoverableURIs.Builder() ).Build();

			  empty.ForEach( _consumer );

			  verify( _consumer, never() ).accept(anyString(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeConsumerForEachKey() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeConsumerForEachKey()
		 {
			  DiscoverableURIs discoverables = ( new DiscoverableURIs.Builder() ).add("a", "/test", NORMAL).add("b", "/data", NORMAL).add("c", "http://www.example.com", LOW).Build();

			  discoverables.ForEach( _consumer );

			  verify( _consumer, times( 1 ) ).accept( "a", new URI( "/test" ) );
			  verify( _consumer, times( 1 ) ).accept( "b", new URI( "/data" ) );
			  verify( _consumer, times( 1 ) ).accept( "c", new URI( "http://www.example.com" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenAddingTwoEntriesWithSamePrecedence()
		 public virtual void ShouldThrowWhenAddingTwoEntriesWithSamePrecedence()
		 {
			  try
			  {
					DiscoverableURIs discoverables = ( new DiscoverableURIs.Builder() ).add("a", "/test", NORMAL).add("a", "/data", NORMAL).Build();

					fail( "exception expected" );
			  }
			  catch ( Exception t )
			  {
					assertThat( t, @is( instanceOf( typeof( InvalidSettingException ) ) ) );
					assertThat( t.Message, startsWith( "Unable to add two entries with the same precedence using key " ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeConsumerForEachKeyWithHighestPrecedence() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeConsumerForEachKeyWithHighestPrecedence()
		 {
			  DiscoverableURIs discoverables = ( new DiscoverableURIs.Builder() ).add("c", "bolt://localhost:7687", HIGHEST).add("a", "/test", NORMAL).add("b", "/data", NORMAL).add("b", "/data2", LOWEST).add("a", "/test2", HIGHEST).add("c", "bolt://localhost:7688", LOW).Build();

			  discoverables.ForEach( _consumer );

			  verify( _consumer, times( 1 ) ).accept( "a", new URI( "/test2" ) );
			  verify( _consumer, times( 1 ) ).accept( "b", new URI( "/data" ) );
			  verify( _consumer, times( 1 ) ).accept( "c", new URI( "bolt://localhost:7687" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeConsumerForEachKeyWithHighestPrecedenceOnce() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeConsumerForEachKeyWithHighestPrecedenceOnce()
		 {
			  DiscoverableURIs discoverables = ( new DiscoverableURIs.Builder() ).add("a", "/test1", LOWEST).add("a", "/test2", LOW).add("a", "/data3", NORMAL).add("a", "/test4", HIGH).add("a", "/test5", HIGHEST).Build();

			  discoverables.ForEach( _consumer );

			  verify( _consumer, only() ).accept("a", new URI("/test5"));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertStringIntoURI() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertStringIntoURI()
		 {
			  DiscoverableURIs empty = ( new DiscoverableURIs.Builder() ).add("a", "bolt://localhost:7687", NORMAL).Build();

			  empty.ForEach( _consumer );

			  verify( _consumer, times( 1 ) ).accept( "a", new URI( "bolt://localhost:7687" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertSchemeHostPortIntoURI() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertSchemeHostPortIntoURI()
		 {
			  DiscoverableURIs empty = ( new DiscoverableURIs.Builder() ).Add("a", "bolt", "www.example.com", 8888, NORMAL).build();

			  empty.ForEach( _consumer );

			  verify( _consumer, times( 1 ) ).accept( "a", new URI( "bolt://www.example.com:8888" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUsePassedURI() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUsePassedURI()
		 {
			  URI uri = new URI( "bolt://www.example.com:9999" );

			  DiscoverableURIs empty = ( new DiscoverableURIs.Builder() ).add("a", uri, NORMAL).Build();

			  empty.ForEach( _consumer );

			  verify( _consumer, times( 1 ) ).accept( "a", uri );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverrideLowestForAbsolute() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOverrideLowestForAbsolute()
		 {
			  URI @override = new URI( "http://www.example.com:9999" );
			  DiscoverableURIs empty = ( new DiscoverableURIs.Builder() ).add("a", "bolt://localhost:8989", LOWEST).OverrideAbsolutesFromRequest(@override).build();

			  empty.ForEach( _consumer );

			  verify( _consumer, times( 1 ) ).accept( "a", new URI( "bolt://www.example.com:8989" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOverrideOtherThanLowestForAbsolute() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotOverrideOtherThanLowestForAbsolute()
		 {
			  URI @override = new URI( "http://www.example.com:9999" );
			  DiscoverableURIs empty = ( new DiscoverableURIs.Builder() ).add("a", "bolt://localhost:8989", LOW).add("b", "bolt://localhost:8990", NORMAL).add("c", "bolt://localhost:8991", HIGH).add("d", "bolt://localhost:8992", HIGHEST).OverrideAbsolutesFromRequest(@override).build();

			  empty.ForEach( _consumer );

			  verify( _consumer, times( 1 ) ).accept( "a", new URI( "bolt://localhost:8989" ) );
			  verify( _consumer, times( 1 ) ).accept( "b", new URI( "bolt://localhost:8990" ) );
			  verify( _consumer, times( 1 ) ).accept( "c", new URI( "bolt://localhost:8991" ) );
			  verify( _consumer, times( 1 ) ).accept( "d", new URI( "bolt://localhost:8992" ) );
		 }
	}

}