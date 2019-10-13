﻿using System;

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
namespace Neo4Net.Test.extension
{
	using AfterAllCallback = org.junit.jupiter.api.extension.AfterAllCallback;
	using BeforeAllCallback = org.junit.jupiter.api.extension.BeforeAllCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;

	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

	public class TestDirectoryClassExtension : BeforeAllCallback, AfterAllCallback
	{
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beforeAll(org.junit.jupiter.api.extension.ExtensionContext context) throws Exception
		 public override void BeforeAll( ExtensionContext context )
		 {
			  _testDirectory = TestDirectory.testDirectory();
			  Type testClass = context.RequiredTestClass;
			  string testName = context.RequiredTestClass.Name;
			  _testDirectory.prepareDirectory( testClass, testName );
		 }

		 public virtual TestDirectory TestDirectory
		 {
			 get
			 {
				  return _testDirectory;
			 }
		 }

		 public override void AfterAll( ExtensionContext context )
		 {
			  try
			  {
					_testDirectory.complete( !context.ExecutionException.Present );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}