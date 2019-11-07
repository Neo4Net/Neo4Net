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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.TEMPORAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.values;

	public class LazyInstanceSelectorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstantiateLazilyOnFirstSelect()
		 public virtual void ShouldInstantiateLazilyOnFirstSelect()
		 {
			  // given
			  System.Func<IndexSlot, string> factory = SlotToStringFunction();
			  LazyInstanceSelector<string> selector = new LazyInstanceSelector<string>( factory );

			  // when
			  foreach ( IndexSlot slot in values() )
			  {
					foreach ( IndexSlot candidate in values() )
					{
						 // then
						 if ( ( int )candidate < ( int )slot )
						 {
							  verify( factory, times( 1 ) ).apply( candidate );
							  selector.Select( candidate );
							  verify( factory, times( 1 ) ).apply( candidate );
						 }
						 else if ( candidate == slot )
						 {
							  verify( factory, times( 0 ) ).apply( candidate );
							  selector.Select( candidate );
							  verify( factory, times( 1 ) ).apply( candidate );
						 }
						 else
						 {
							  assertNull( selector.GetIfInstantiated( candidate ) );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformActionOnAll()
		 public virtual void ShouldPerformActionOnAll()
		 {
			  // given
			  System.Func<IndexSlot, string> factory = SlotToStringFunction();
			  LazyInstanceSelector<string> selector = new LazyInstanceSelector<string>( factory );
			  selector.Select( STRING );

			  // when
			  System.Action<string> consumer = mock( typeof( System.Action ) );
			  selector.ForAll( consumer );

			  // then
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					verify( consumer, times( 1 ) ).accept( slot.ToString() );
			  }
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAllInstantiated()
		 public virtual void ShouldCloseAllInstantiated()
		 {
			  // given
			  System.Func<IndexSlot, string> factory = SlotToStringFunction();
			  LazyInstanceSelector<string> selector = new LazyInstanceSelector<string>( factory );
			  selector.Select( NUMBER );
			  selector.Select( STRING );

			  // when
			  System.Action<string> consumer = mock( typeof( System.Action ) );
			  selector.Close( consumer );

			  // then
			  verify( consumer, times( 1 ) ).accept( NUMBER.ToString() );
			  verify( consumer, times( 1 ) ).accept( STRING.ToString() );
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPreventInstantiationAfterClose()
		 public virtual void ShouldPreventInstantiationAfterClose()
		 {
			  // given
			  System.Func<IndexSlot, string> factory = SlotToStringFunction();
			  LazyInstanceSelector<string> selector = new LazyInstanceSelector<string>( factory );
			  selector.Select( NUMBER );
			  selector.Select( STRING );

			  // when
			  selector.Close( mock( typeof( System.Action ) ) );

			  // then
			  try
			  {
					selector.Select( TEMPORAL );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then good
			  }
		 }

		 private System.Func<IndexSlot, string> SlotToStringFunction()
		 {
			  System.Func<IndexSlot, string> factory = mock( typeof( System.Func ) );
			  when( factory( any( typeof( IndexSlot ) ) ) ).then( invocationOnMock => ( ( IndexSlot ) invocationOnMock.getArgument( 0 ) ).ToString() );
			  return factory;
		 }
	}

}