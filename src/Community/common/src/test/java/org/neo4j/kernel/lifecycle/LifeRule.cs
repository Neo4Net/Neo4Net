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
namespace Neo4Net.Kernel.Lifecycle
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;

	/// <summary>
	///  JUnit rule that allows you to manage lifecycle of a set of instances. Register instances
	///  and then use the init/start/stop/shutdown methods.
	/// </summary>
	public class LifeRule : TestRule
	{
		 private LifeSupport _life = new LifeSupport();
		 private readonly bool _autoStart;

		 public LifeRule() : this(false)
		 {
		 }

		 public LifeRule( bool autoStart )
		 {
			  this._autoStart = autoStart;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly LifeRule _outerInstance;

			 private Statement @base;

			 public StatementAnonymousInnerClass( LifeRule outerInstance, Statement @base )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  try
				  {
						if ( _outerInstance.autoStart )
						{
							 outerInstance.Start();
						}
						@base.evaluate();
						_outerInstance.life.shutdown();
				  }
				  catch ( Exception failure )
				  {
						try
						{
							 _outerInstance.life.shutdown();
						}
						catch ( Exception suppressed )
						{
							 failure.addSuppressed( suppressed );
						}
						throw failure;
				  }
				  finally
				  {
						_outerInstance.life = new LifeSupport();
				  }
			 }
		 }

		 public virtual T Add<T>( T instance ) where T : Lifecycle
		 {
			  return _life.add( instance );
		 }

		 public virtual void Init()
		 {
			  _life.init();
		 }

		 public virtual void Start()
		 {
			  _life.start();
		 }

		 public virtual void Stop()
		 {
			  _life.stop();
		 }

		 public virtual void Shutdown()
		 {
			  _life.shutdown();
		 }
	}

}