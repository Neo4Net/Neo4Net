using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise.id
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using IdTypeConfiguration = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfiguration;
	using IdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class EnterpriseIdTypeConfigurationProviderTest
	public class EnterpriseIdTypeConfigurationProviderTest
	{
		 private readonly IdType _reusableType;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.List<org.Neo4Net.kernel.impl.store.id.IdType> data()
		 public static IList<IdType> Data()
		 {
			  return Arrays.asList( IdType.PROPERTY, IdType.STRING_BLOCK, IdType.ARRAY_BLOCK, IdType.NODE, IdType.RELATIONSHIP, IdType.NODE_LABELS );
		 }

		 public EnterpriseIdTypeConfigurationProviderTest( IdType reusableType )
		 {
			  this._reusableType = reusableType;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonReusableTypeConfiguration()
		 public virtual void NonReusableTypeConfiguration()
		 {
			  IdTypeConfigurationProvider provider = CreateIdTypeProvider();
			  IdTypeConfiguration typeConfiguration = provider.GetIdTypeConfiguration( IdType.SCHEMA );
			  assertFalse( "Schema record ids are not reusable.", typeConfiguration.AllowAggressiveReuse() );
			  assertEquals( "Schema record ids are not reusable.", 1024, typeConfiguration.GrabSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reusableTypeConfiguration()
		 public virtual void ReusableTypeConfiguration()
		 {
			  IdTypeConfigurationProvider provider = CreateIdTypeProvider();
			  IdTypeConfiguration typeConfiguration = provider.GetIdTypeConfiguration( _reusableType );
			  assertTrue( typeConfiguration.AllowAggressiveReuse() );
			  assertEquals( 50000, typeConfiguration.GrabSize );
		 }

		 private IdTypeConfigurationProvider CreateIdTypeProvider()
		 {
			  IDictionary<string, string> @params = MapUtil.stringMap( EnterpriseEditionSettings.idTypesToReuse.name(), IdType.NODE + "," + IdType.RELATIONSHIP );
			  Config config = Config.defaults( @params );
			  return new EnterpriseIdTypeConfigurationProvider( config );
		 }
	}

}