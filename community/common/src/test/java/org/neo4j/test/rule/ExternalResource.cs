﻿using System;

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
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;

	/// <summary>
	/// A better version of <seealso cref="org.junit.rules.ExternalResource"/> that properly handles exceptions in {@link
	/// #after(boolean)}.
	/// </summary>
	public abstract class ExternalResource : TestRule
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly ExternalResource _outerInstance;

			 private Statement @base;

			 public StatementAnonymousInnerClass( ExternalResource outerInstance, Statement @base )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  outerInstance.Before();
				  Exception failure = null;
				  try
				  {
						@base.evaluate();
				  }
				  catch ( Exception e )
				  {
						failure = e;
				  }
				  finally
				  {
						try
						{
							 outerInstance.After( failure == null );
						}
						catch ( Exception e )
						{
							 if ( failure != null )
							 {
								  failure.addSuppressed( e );
							 }
							 else
							 {
								  failure = e;
							 }
						}
				  }
				  if ( failure != null )
				  {
						throw failure;
				  }
			 }
		 }

		 /// <summary>
		 /// Override to set up your specific external resource.
		 /// </summary>
		 /// <exception cref="Throwable"> if setup fails (which will disable {@code after} </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void before() throws Throwable
		 protected internal virtual void Before()
		 {
			  // do nothing
		 }

		 /// <summary>
		 /// Override to tear down your specific external resource.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void after(boolean successful) throws Throwable
		 protected internal virtual void After( bool successful )
		 {
			  // do nothing
		 }
	}

}