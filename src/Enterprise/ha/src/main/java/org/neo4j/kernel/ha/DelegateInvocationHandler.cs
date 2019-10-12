using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.Kernel.ha
{

	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using TransientDatabaseFailureException = Neo4Net.Graphdb.TransientDatabaseFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Neo4Net.Kernel.impl.util;

	/// <summary>
	/// InvocationHandler for dynamic proxies that delegate calls to a given backing implementation. This is mostly
	/// used to present a single object to others, while being able to switch implementation at runtime.
	/// 
	/// <seealso cref="cement()"/>: acquire a proxy that will have its delegate assigned the next call to
	/// <seealso cref="setDelegate(object)"/>. This is useful if one <seealso cref="DelegateInvocationHandler"/> depends on
	/// another which will have its delegate set later than this one.
	/// </summary>
	public class DelegateInvocationHandler<T> : InvocationHandler
	{
		 private volatile T @delegate;

		 // A concrete version of delegate, where a user can request to cement this delegate so that it gets concrete
		 // the next call to setDelegate and will never change since.
		 private readonly LazySingleReference<T> _concrete;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public DelegateInvocationHandler(final Class<T> interfaceClass)
		 public DelegateInvocationHandler( Type interfaceClass )
		 {
				 interfaceClass = typeof( T );
			  _concrete = new LazySingleReferenceAnonymousInnerClass( this, interfaceClass );
		 }

		 private class LazySingleReferenceAnonymousInnerClass : LazySingleReference<T>
		 {
			 private readonly DelegateInvocationHandler<T> _outerInstance;

			 private Type _interfaceClass;

			 public LazySingleReferenceAnonymousInnerClass( DelegateInvocationHandler<T> outerInstance, Type interfaceClass )
			 {
				 this.outerInstance = outerInstance;
				 this._interfaceClass = interfaceClass;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override protected T create()
			 protected internal override T create()
			 {
				  return ( T ) Proxy.newProxyInstance( typeof( DelegateInvocationHandler ).ClassLoader, new Type[] { _interfaceClass }, new Concrete<>() );
			 }
		 }

		 /// <summary>
		 /// Updates the delegate for this handler, also <seealso cref="harden() hardens"/> instances
		 /// <seealso cref="cement() cemented"/> from the last call to <seealso cref="setDelegate(object)"/>.
		 /// This call will also dereference the <seealso cref="DelegateInvocationHandler.Concrete"/>,
		 /// such that future calls to <seealso cref="harden()"/> cannot affect any reference received
		 /// from <seealso cref="cement()"/> prior to this call. </summary>
		 /// <param name="delegate"> the new delegate to set.
		 /// </param>
		 /// <returns> the old delegate </returns>
		 public virtual T setDelegate( T @delegate )
		 {
			  T oldDelegate = this.@delegate;
			  this.@delegate = @delegate;
			  Harden();
			  _concrete.invalidate();
			  return oldDelegate;
		 }

		 /// <summary>
		 /// Updates <seealso cref="cement() cemented"/> delegates with the current delegate, making it concrete.
		 /// Callers of <seealso cref="cement()"/> in between this call and the previous call to <seealso cref="setDelegate(object)"/>
		 /// will see the current delegate.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") void harden()
		 internal virtual void Harden()
		 {
			  ( ( Concrete<T> ) Proxy.getInvocationHandler( _concrete.get() ) ).set(@delegate);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object invoke(Object proxy, Method method, Object[] args) throws Throwable
		 public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
		 {
			  if ( @delegate == default( T ) )
			  {
					throw new StateChangedTransactionFailureException( "This transaction made assumptions about the instance it is executing " + "on that no longer hold true. This normally happens when a transaction " + "expects the instance it is executing on to be in some specific cluster role" + "(such as 'master' or 'slave') and the instance " + "changing state while the transaction is executing. Simply retry your " + "transaction and you should see a successful outcome." );
			  }
			  return ProxyInvoke( @delegate, method, args );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Object proxyInvoke(Object delegate, Method method, Object[] args) throws Throwable
		 private static object ProxyInvoke( object @delegate, System.Reflection.MethodInfo method, object[] args )
		 {
			  try
			  {
					return method.invoke( @delegate, args );
			  }
			  catch ( InvocationTargetException e )
			  {
					throw e.InnerException;
			  }
		 }

		 /// <summary>
		 /// Cements this delegate, i.e. returns an instance which will have its delegate assigned and hardened
		 /// later on so that it never will change after that point.
		 /// </summary>
		 public virtual T Cement()
		 {
			  return _concrete.get();
		 }

		 public override string ToString()
		 {
			  return "Delegate[" + @delegate + "]";
		 }

		 private class Concrete<T> : InvocationHandler
		 {
			  internal volatile T Delegate;

			  internal virtual void Set( T @delegate )
			  {
					this.Delegate = @delegate;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object invoke(Object proxy, Method method, Object[] args) throws Throwable
			  public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					if ( Delegate == default( T ) )
					{
						 throw new TransientDatabaseFailureException( "Instance state is not valid. There is no master currently available. Possible causes " + "include unavailability of a majority of the cluster members or network failure " + "that caused this instance to be partitioned away from the cluster" );
					}

					return ProxyInvoke( Delegate, method, args );
			  }

			  public override string ToString()
			  {
					return "Concrete[" + Delegate + "]";
			  }
		 }

		 /// <summary>
		 /// Because we don't want the public API to implement `HasStatus`, and because
		 /// we don't want to change the API from throwing `TransactionFailureException` for
		 /// backwards compat reasons, we throw this sub-class that adds a status code.
		 /// </summary>
		 internal class StateChangedTransactionFailureException : TransactionFailureException, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
		 {
			  internal StateChangedTransactionFailureException( string msg ) : base( msg )
			  {
			  }

			  public override Status Status()
			  {
					return Neo4Net.Kernel.Api.Exceptions.Status_Transaction.InstanceStateChanged;
			  }
		 }
	}

}