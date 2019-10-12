using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.impl.enterprise.configuration
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Test = org.junit.Test;


	using InvalidSettingException = Org.Neo4j.Graphdb.config.InvalidSettingException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.idTypesToReuse;

	public class EnterpriseEditionSettingsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void idTypesToReuseAllowedValues()
		 public virtual void IdTypesToReuseAllowedValues()
		 {
			  foreach ( IdType type in Enum.GetValues( typeof( IdType ) ) )
			  {
					if ( type == IdType.NODE || type == IdType.RELATIONSHIP )
					{
						 AssertIdTypesToReuseAllows( type );
					}
					else
					{
						 AssertIdTypesToReuseDisallows( type );
					}
			  }

			  AssertIdTypesToReuseAllows( IdType.NODE, IdType.RELATIONSHIP );
			  AssertIdTypesToReuseAllows( IdType.RELATIONSHIP, IdType.NODE );

			  AssertIdTypesToReuseDisallows( IdType.NODE, IdType.RELATIONSHIP, IdType.RELATIONSHIP_GROUP );
			  AssertIdTypesToReuseDisallows( IdType.SCHEMA, IdType.NEOSTORE_BLOCK );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void idTypesToReuseCaseInsensitive()
		 public virtual void IdTypesToReuseCaseInsensitive()
		 {
			  Config config1 = Config.defaults( idTypesToReuse, "node, relationship" );
			  assertEquals( AsList( IdType.NODE, IdType.RELATIONSHIP ), config1.Get( idTypesToReuse ) );

			  Config config2 = Config.defaults( idTypesToReuse, "rElAtIoNshiP, NoDe" );
			  assertEquals( AsList( IdType.RELATIONSHIP, IdType.NODE ), config2.Get( idTypesToReuse ) );
		 }

		 private static void AssertIdTypesToReuseAllows( IdType type, params IdType[] otherTypes )
		 {
			  Config config = ConfigWithIdTypes( type, otherTypes );
			  IList<IdType> types = config.Get( idTypesToReuse );
			  assertEquals( AsList( type, otherTypes ), types );
		 }

		 private static void AssertIdTypesToReuseDisallows( IdType type, params IdType[] otherTypes )
		 {
			  try
			  {
					Config config = ConfigWithIdTypes( type, otherTypes );
					config.Get( idTypesToReuse );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( InvalidSettingException ) ) );
			  }
		 }

		 private static Config ConfigWithIdTypes( IdType type, params IdType[] otherTypes )
		 {
			  string value = StringList( type, otherTypes );
			  return Config.defaults( idTypesToReuse, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static <T> String stringList(T element, T... elements)
		 private static string StringList<T>( T element, params T[] elements )
		 {
			  return StringUtils.join( AsList( element, elements ), "," );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static <T> java.util.List<T> asList(T element, T... elements)
		 private static IList<T> AsList<T>( T element, params T[] elements )
		 {
			  IList<T> list = new List<T>();
			  list.Add( element );
			  Collections.addAll( list, elements );
			  return list;
		 }
	}

}