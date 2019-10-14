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
namespace Neo4Net.Test.rule
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	/// <summary>
	/// Set a test to loop a number of times. If you find yourself using this in a production test, you are probably doing
	/// something wrong.
	/// 
	/// However, as a temporary measure used locally, it serves as an excellent tool to trigger errors in flaky tests.
	/// 
	/// If used together with other TestRules, usually you need to make sure that this is placed as the LAST field in the
	/// test class. This is to ensure that all other rules run inside of this rule, for example to start/stop databases and other resources.
	/// </summary>
	public class RepeatRule : TestRule
	{
		 [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		 public class Repeat : System.Attribute
		 {
			 private readonly RepeatRule _outerInstance;

			 public Repeat;
			 {
			 }

			  internal int times;

			 public Repeat( public Repeat, int times )
			 {
				 this.Repeat = Repeat;
				 this.times = times;
			 }
		 }

		 private readonly bool _printRepeats;
		 private readonly int _defaultTimes;

		 private int _count;

		 public RepeatRule() : this(false, 1)
		 {
		 }

		 public RepeatRule( bool printRepeats, int defaultRepeats )
		 {
			  this._printRepeats = printRepeats;
			  this._defaultTimes = defaultRepeats;
		 }

		 private class RepeatStatement : Statement
		 {
			 private readonly RepeatRule _outerInstance;

			  internal readonly int Times;
			  internal readonly Statement Statement;
			  internal readonly string TestName;

			  internal RepeatStatement( RepeatRule outerInstance, int times, Statement statement, Description testDescription )
			  {
				  this._outerInstance = outerInstance;
					this.Times = times;
					this.Statement = statement;
					this.TestName = testDescription.DisplayName;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			  public override void Evaluate()
			  {
					for ( outerInstance.count = 0; outerInstance.count < Times; outerInstance.count++ )
					{
						 if ( outerInstance.printRepeats )
						 {
							  Console.WriteLine( TestName + " iteration " + ( outerInstance.count + 1 ) + "/" + Times );
						 }
						 Statement.evaluate();
					}
			  }
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
			  Repeat repeat = description.getAnnotation( typeof( Repeat ) );
			  if ( repeat != null )
			  {
					return new RepeatStatement( this, repeat.times(), @base, description );
			  }
			  if ( _defaultTimes > 1 )
			  {
					return new RepeatStatement( this, _defaultTimes, @base, description );
			  }
			  return @base;
		 }

		 /// <summary>
		 /// Get the current count. This can be used (for example) in a non-suspending breakpoint at the beginning of a
		 /// test to print out what iteration is currently running.
		 /// </summary>
		 /// <returns> current count </returns>
		 public virtual int Count
		 {
			 get
			 {
				  return _count;
			 }
		 }
	}

}