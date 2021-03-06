﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Test.rule
{
	using ExternalResource = org.junit.rules.ExternalResource;


	/// <summary>
	/// Simple means of cleaning up after a test. It has two purposes:
	/// o remove try-finally blocks from tests, which looks mostly like clutter
	/// o remove @After code needing to clean up after a test
	/// 
	/// Usage:
	/// <pre><code>
	/// public final @Rule CleanupRule cleanup = new CleanupRule();
	/// ...
	/// @Test
	/// public void shouldAssertSomething()
	/// {
	///     SomeObjectThatNeedsClosing dirt = cleanup.add( new SomeObjectThatNeedsClosing() );
	///     ...
	/// }
	/// </code></pre>
	/// 
	/// It accepts <seealso cref="AutoCloseable"/> objects, since it's the lowest denominator for closeables.
	/// And it accepts just about any object, where it tries to spot an appropriate close method, like "close" or "shutdown"
	/// and calls it via reflection.
	/// </summary>
	public class CleanupRule : ExternalResource
	{
		 private static readonly string[] _commonCloseMethodNames = new string[] { "close", "stop", "shutdown", "shutDown" };
		 private readonly LinkedList<AutoCloseable> _toCloseAfterwards = new LinkedList<AutoCloseable>();

		 protected internal override void After()
		 {
			  foreach ( AutoCloseable toClose in _toCloseAfterwards )
			  {
					try
					{
						 toClose.close();
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
			  }
		 }

		 public virtual T Add<T>( T toClose ) where T : AutoCloseable
		 {
			  _toCloseAfterwards.AddFirst( toClose );
			  return toClose;
		 }

		 public virtual T Add<T>( T toClose )
		 {
			  Type cls = toClose.GetType();
			  foreach ( string methodName in _commonCloseMethodNames )
			  {
					try
					{
						 System.Reflection.MethodInfo method = cls.GetMethod( methodName );
						 method.Accessible = true;
						 Add( Closeable( method, toClose ) );
						 return toClose;
					}
					catch ( NoSuchMethodException )
					{
						 // ignore
					}
					catch ( SecurityException e )
					{
						 throw new Exception( e );
					}
			  }
			  throw new System.ArgumentException( "No suitable close method found on " + toClose + ", which is a " + cls );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static AutoCloseable closeable(final Method method, final Object target)
		 private static AutoCloseable Closeable( System.Reflection.MethodInfo method, object target )
		 {
			  return () =>
			  {
				try
				{
					 method.invoke( target );
				}
				catch ( Exception e ) when ( e is IllegalAccessException || e is System.ArgumentException || e is InvocationTargetException )
				{
					 throw new Exception( e );
				}
			  };
		 }
	}

}