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
namespace Neo4Net.Kernel.impl.store
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;

	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class IdGeneratorImplContractTest : IdGeneratorContractTest
	{
		private bool InstanceFieldsInitialized = false;

		public IdGeneratorImplContractTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fsRule.get() );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _testDirectory );
		}

		 private EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(testDirectory);
		 public RuleChain RuleChain;

		 private EphemeralFileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  _fs = _fsRule.get();
		 }

		 protected internal override IdGenerator CreateIdGenerator( int grabSize )
		 {
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  return OpenIdGenerator( grabSize );
		 }

		 protected internal override IdGenerator OpenIdGenerator( int grabSize )
		 {
			  return new IdGeneratorImpl( _fs, IdGeneratorFile(), grabSize, 1000, false, IdType.NODE, () => 0L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void verifyFileCleanup()
		 public virtual void VerifyFileCleanup()
		 {
			  File file = IdGeneratorFile();
			  if ( file.exists() )
			  {
					assertTrue( file.delete() );
			  }
		 }

		 private File IdGeneratorFile()
		 {
			  return _testDirectory.file( "testIdGenerator.id" );
		 }
	}

}