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
namespace Neo4Net.Test.rule
{
	using ExternalResource = org.junit.rules.ExternalResource;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	/// <summary>
	/// Creates a logger for tests, and marks beginning and end of tests with log messages
	/// </summary>
	public class LoggerRule : ExternalResource
	{
		 private readonly Level _level;
		 private Logger _logger;
		 private string _testName;

		 public LoggerRule() : this(Level.INFO)
		 {
		 }

		 public LoggerRule( Level level )
		 {
			  this._level = level;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void before() throws Throwable
		 protected internal override void Before()
		 {
			  _logger.info( "Begin test:" + _testName );
			  base.Before();
		 }

		 protected internal override void After()
		 {
			  base.After();
			  _logger.info( "Finished test:" + _testName );
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
			  _testName = description.DisplayName;
			  _logger = Logger.getLogger( description.TestClass.Name );
			  _logger.Level = _level;
			  return base.Apply( @base, description );
		 }

		 public virtual Logger Logger
		 {
			 get
			 {
				  return _logger;
			 }
		 }
	}

}