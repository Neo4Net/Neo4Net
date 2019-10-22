using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Test = org.junit.Test;

	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using TransientDatabaseFailureException = Neo4Net.GraphDb.TransientDatabaseFailureException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class DelegateInvocationHandlerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToUseValueBeforeHardened()
		 public virtual void ShouldNotBeAbleToUseValueBeforeHardened()
		 {
			  // GIVEN
			  DelegateInvocationHandler<Value> handler = NewDelegateInvocationHandler();
			  Value value = handler.Cement();

			  // WHEN
			  try
			  {
					value.Get();
					fail( "Should fail" );
			  }
			  catch ( Exception e )
			  {
					// THEN
					assertThat( e, instanceOf( typeof( TransientDatabaseFailureException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenDelegateIsNotSet()
		 public virtual void ThrowsWhenDelegateIsNotSet()
		 {
			  DelegateInvocationHandler<Value> handler = NewDelegateInvocationHandler();

			  try
			  {
					handler.Invoke( new object(), typeof(Value).getDeclaredMethod("get"), new object[0] );
					fail( "Exception expected" );
			  }
			  catch ( Exception t )
			  {
					assertThat( t, instanceOf( typeof( TransactionFailureException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseCementedValueOnceDelegateSet()
		 public virtual void ShouldBeAbleToUseCementedValueOnceDelegateSet()
		 {
			  // GIVEN
			  DelegateInvocationHandler<Value> handler = NewDelegateInvocationHandler();
			  Value cementedValue = handler.Cement();

			  // WHEN setting the delegate (implies hardening it)
			  handler.Delegate = Value( 10 );

			  // THEN
			  assertEquals( 10, cementedValue.Get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseCementedValueOnceHardened()
		 public virtual void ShouldBeAbleToUseCementedValueOnceHardened()
		 {
			  // GIVEN
			  DelegateInvocationHandler<Value> handler = NewDelegateInvocationHandler();
			  Value cementedValue = handler.Cement();

			  // WHEN setting the delegate (implies hardening it)
			  handler.Delegate = Value( 10 );

			  // THEN
			  assertEquals( 10, cementedValue.Get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setDelegateShouldBeAbleToOverridePreviousHarden()
		 public virtual void SetDelegateShouldBeAbleToOverridePreviousHarden()
		 {
			  /* This test case stems from a race condition where a thread switching role to slave and
			   * HaKernelPanicHandler thread were competing to harden the master delegate handler.
			   * While all components were switching to their slave versions a kernel panic event came
			   * in and made its switch, ending with hardening the master delegate handler. All components
			   * would take that as a sign about the master delegate being ready to use. When the slave switching
			   * was done, that thread would set the master delegate to the actual one, but all components would
			   * have already gotten a concrete reference to the master such that the latter setDelegate call would
			   * not affect them. */

			  // GIVEN
			  DelegateInvocationHandler<Value> handler = NewDelegateInvocationHandler();
			  handler.Delegate = Value( 10 );
			  Value cementedValue = handler.Cement();
			  handler.Harden();

			  // WHEN setting the delegate (implies hardening it)
			  handler.Delegate = Value( 20 );

			  // THEN
			  assertEquals( 20, cementedValue.Get() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Value value(final int i)
		 private Value Value( int i )
		 {
			  return () => i;
		 }

		 private static DelegateInvocationHandler<Value> NewDelegateInvocationHandler()
		 {
			  return new DelegateInvocationHandler<Value>( typeof( Value ) );
		 }

		 private interface Value
		 {
			  int Get();
		 }
	}

}