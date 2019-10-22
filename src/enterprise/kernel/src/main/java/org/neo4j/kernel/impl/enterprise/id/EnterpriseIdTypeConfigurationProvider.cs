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
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using CommunityIdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using IdTypeConfiguration = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfiguration;

	/// <summary>
	/// Id type configuration provider for enterprise edition.
	/// Allow to reuse predefined id types that are reused in community and in
	/// addition to that allow additional id types to reuse be specified by
	/// <seealso cref="EnterpriseEditionSettings.idTypesToReuse"/> setting.
	/// </summary>
	/// <seealso cref= IdType </seealso>
	/// <seealso cref= IdTypeConfiguration </seealso>
	public class EnterpriseIdTypeConfigurationProvider : CommunityIdTypeConfigurationProvider
	{

		 private readonly ISet<IdType> _typesToReuse;

		 public EnterpriseIdTypeConfigurationProvider( Config config )
		 {
			  _typesToReuse = ConfigureReusableTypes( config );
		 }

		 protected internal override ISet<IdType> TypesToReuse
		 {
			 get
			 {
				  return _typesToReuse;
			 }
		 }

		 private EnumSet<IdType> ConfigureReusableTypes( Config config )
		 {
			  EnumSet<IdType> types = EnumSet.copyOf( base.TypesToReuse );
			  IList<IdType> configuredTypes = config.Get( EnterpriseEditionSettings.idTypesToReuse );
			  types.addAll( configuredTypes );
			  return types;
		 }
	}

}