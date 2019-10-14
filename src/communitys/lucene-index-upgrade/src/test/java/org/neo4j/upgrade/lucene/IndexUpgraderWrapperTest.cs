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
namespace Neo4Net.Upgrade.Lucene
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using EmbeddedJarLoader = Neo4Net.Upgrade.Loader.EmbeddedJarLoader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IndexUpgraderWrapperTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexUpgraderInvokesLuceneMigrator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexUpgraderInvokesLuceneMigrator()
		 {
			  IndexUpgraderWrapper upgrader = GetIndexUpgrader( CreateJarLoader() );

			  UpgraderStub.ResetInvocationMark();
			  upgrader.UpgradeIndex( Paths.get( "some" ) );
			  assertTrue( UpgraderStub.InvocationMark );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexUpgraderReleaseResourcesOnClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexUpgraderReleaseResourcesOnClose()
		 {
			  EmbeddedJarLoader jarLoader = CreateJarLoader();
			  IndexUpgraderWrapper upgrader = GetIndexUpgrader( jarLoader );

			  upgrader.UpgradeIndex( Paths.get( "some" ) );
			  upgrader.Close();

			  verify( jarLoader ).close();
		 }

		 private IndexUpgraderWrapper GetIndexUpgrader( EmbeddedJarLoader jarLoader )
		 {
			  return new IndexUpgraderWrapper( () => jarLoader );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.upgrade.loader.EmbeddedJarLoader createJarLoader() throws ClassNotFoundException, java.io.IOException
		 private EmbeddedJarLoader CreateJarLoader()
		 {
			  EmbeddedJarLoader jarLoader = Mockito.mock( typeof( EmbeddedJarLoader ) );
			  when( jarLoader.LoadEmbeddedClass( anyString() ) ).thenReturn(typeof(UpgraderStub));
			  return jarLoader;
		 }
	}

}