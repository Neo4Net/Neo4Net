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
namespace Neo4Net.Test.randomized
{

	using Neo4Net.Test.randomized.RandomizedTester;

	public class TestCaseWriter<T, F>
	{
		 private readonly string _testName;
		 private readonly Printable _given;
		 private readonly IList<Action<T, F>> _actions;
		 private readonly Action<T, F> _failingAction;
		 private readonly TargetFactory<T> _targetFactory;

		 internal TestCaseWriter( string testName, Printable given, TargetFactory<T> targetFactory, IList<Action<T, F>> actions, Action<T, F> failingAction )
		 {
			  this._testName = testName;
			  this._given = given;
			  this._targetFactory = targetFactory;
			  this._actions = actions;
			  this._failingAction = failingAction;
		 }

		 public virtual void Print( PrintStream @out )
		 {
			  T target = _targetFactory.newInstance();
			  LinePrinter baseLinePrinter = new PrintStreamLinePrinter( @out, 0 );
			  baseLinePrinter.Println( "@Test" );
			  baseLinePrinter.Println( "public void " + _testName + "() throws Exception" );
			  baseLinePrinter.Println( "{" );

			  LinePrinter codePrinter = baseLinePrinter.Indent();
			  codePrinter.Println( "// GIVEN" );
			  _given.print( codePrinter );
			  foreach ( Action<T, F> action in _actions )
			  {
					action.PrintAsCode( target, codePrinter, false );
					action.Apply( target );
			  }

			  codePrinter.Println( "" );
			  codePrinter.Println( "// WHEN/THEN" );
			  _failingAction.printAsCode( target, codePrinter, true );
			  baseLinePrinter.Println( "}" );
			  @out.flush();
		 }
	}

}