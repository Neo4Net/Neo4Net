using System.Collections.Generic;

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
namespace Neo4Net.Test.rule
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

	public sealed class Resources : TestRule
	{
		private bool InstanceFieldsInitialized = false;

		public Resources()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.TestDirectoryConflict( _fs );
		}

		 [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		 public class Life : System.Attribute
		 {
			 private readonly Resources _outerInstance;

			 public Life;
			 {
			 }

			  internal InitialLifecycle value;

			 public Life( public Life, InitialLifecycle value )
			 {
				 this.Life = Life;
				 this.value = value;
			 }
		 }

		 public abstract class InitialLifecycle
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INITIALIZED { void initialize(org.neo4j.kernel.lifecycle.LifeRule life) { life.init(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           STARTED { void initialize(org.neo4j.kernel.lifecycle.LifeRule life) { life.start(); } };

			  private static readonly IList<InitialLifecycle> valueList = new List<InitialLifecycle>();

			  static InitialLifecycle()
			  {
				  valueList.Add( INITIALIZED );
				  valueList.Add( STARTED );
			  }

			  public enum InnerEnum
			  {
				  INITIALIZED,
				  STARTED
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private InitialLifecycle( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract void initialize( Neo4Net.Kernel.Lifecycle.LifeRule life );

			 public static IList<InitialLifecycle> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static InitialLifecycle valueOf( string name )
			 {
				 foreach ( InitialLifecycle enumInstance in InitialLifecycle.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private TestDirectory _testDirectory;
		 private readonly LifeRule _life = new LifeRule();

		 public override Statement Apply( Statement @base, Description description )
		 {
			  return _fs.apply( _testDirectory.apply( _pageCacheRule.apply( LifeStatement( @base, description ), description ), description ), description );
		 }

		 private Statement LifeStatement( Statement @base, Description description )
		 {
			  Life initialLifecycle = description.getAnnotation( typeof( Life ) );
			  if ( initialLifecycle != null )
			  {
					@base = Initialise( @base, initialLifecycle.value() );
			  }
			  return _life.apply( @base, description );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.junit.runners.model.Statement initialise(final org.junit.runners.model.Statement super, final InitialLifecycle initialLifecycle)
		 private Statement Initialise( Statement @base, InitialLifecycle initialLifecycle )
		 {
			  return new StatementAnonymousInnerClass( this, @base, initialLifecycle );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly Resources _outerInstance;

			 private Statement @base;
			 private Neo4Net.Test.rule.Resources.InitialLifecycle _initialLifecycle;

			 public StatementAnonymousInnerClass( Resources outerInstance, Statement @base, Neo4Net.Test.rule.Resources.InitialLifecycle initialLifecycle )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._initialLifecycle = initialLifecycle;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  _initialLifecycle.initialize( _outerInstance.life );
				  @base.evaluate();
			 }
		 }

		 public FileSystemAbstraction FileSystem()
		 {
			  return _fs.get();
		 }

		 public PageCache PageCache()
		 {
			  return _pageCacheRule.getPageCache( FileSystem() );
		 }

		 public TestDirectory TestDirectory()
		 {
			  return _testDirectory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void lifeStarts() throws org.neo4j.kernel.lifecycle.LifecycleException
		 public void LifeStarts()
		 {
			  _life.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void lifeShutsDown() throws org.neo4j.kernel.lifecycle.LifecycleException
		 public void LifeShutsDown()
		 {
			  _life.shutdown();
		 }

		 public T Managed<T>( T service )
		 {
			  Lifecycle lifecycle = null;
			  if ( service is Lifecycle )
			  {
					lifecycle = ( Lifecycle ) service;
			  }
			  else if ( service is AutoCloseable )
			  {
					lifecycle = new Closer( ( AutoCloseable ) service );
			  }
			  _life.add( lifecycle );
			  return service;
		 }

		 public LogProvider LogProvider()
		 {
			  return NullLogProvider.Instance;
		 }

		 private class Closer : LifecycleAdapter
		 {
			  internal readonly AutoCloseable Closeable;

			  internal Closer( AutoCloseable closeable )
			  {
					this.Closeable = closeable;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Exception
			  public override void Shutdown()
			  {
					Closeable.close();
			  }
		 }
	}

}