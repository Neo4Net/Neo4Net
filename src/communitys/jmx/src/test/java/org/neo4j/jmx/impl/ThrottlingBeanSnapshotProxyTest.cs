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
namespace Neo4Net.Jmx.impl
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.@internal.verification.VerificationModeFactory.times;

	public class ThrottlingBeanSnapshotProxyTest
	{
		private bool InstanceFieldsInitialized = false;

		public ThrottlingBeanSnapshotProxyTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_proxy = ThrottlingBeanSnapshotProxy.NewThrottlingBeanSnapshotProxy( typeof( TestBean ), _target, 100, _clock );
		}

		 private readonly Clock _clock = mock( typeof( Clock ) );
		 private readonly TestBean _target = mock( typeof( TestBean ) );
		 private TestBean _proxy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotProxyIfUpdateIntervalIsZero()
		 public virtual void DoNotProxyIfUpdateIntervalIsZero()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TestBean result = ThrottlingBeanSnapshotProxy.newThrottlingBeanSnapshotProxy(TestBean.class, target, 0, clock);
			  TestBean result = ThrottlingBeanSnapshotProxy.NewThrottlingBeanSnapshotProxy( typeof( TestBean ), _target, 0, _clock );
			  assertSame( _target, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throttleGetterInvocation()
		 public virtual void ThrottleGetterInvocation()
		 {
			  when( _clock.millis() ).thenReturn(100L);
			  _proxy.Long;
			  _proxy.Long;
			  verify( _target, times( 1 ) ).Long;

			  when( _clock.millis() ).thenReturn(199L);
			  _proxy.Long;
			  verify( _target, times( 1 ) ).Long;

			  when( _clock.millis() ).thenReturn(200L);
			  _proxy.Long;
			  _proxy.Long;
			  verify( _target, times( 2 ) ).Long;
			  verifyNoMoreInteractions( _target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dontThrottleMethodsReturningVoid()
		 public virtual void DontThrottleMethodsReturningVoid()
		 {
			  when( _clock.millis() ).thenReturn(100L);
			  _proxy.returnVoid();
			  _proxy.returnVoid();
			  verify( _target, times( 2 ) ).returnVoid();
			  verifyNoMoreInteractions( _target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dontThrottleMethodsWithArgs()
		 public virtual void DontThrottleMethodsWithArgs()
		 {
			  when( _clock.millis() ).thenReturn(100L);
			  _proxy.notGetter( 1 );
			  _proxy.notGetter( 2 );
			  verify( _target, times( 2 ) ).notGetter( anyLong() );
			  verifyNoMoreInteractions( _target );
		 }

		 private interface TestBean
		 {

			  void ReturnVoid();

			  long Long { get; }

			  long NotGetter( long x );
		 }
	}

}