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
namespace Neo4Net.Kernel.impl.store.id.configuration
{

	/// <summary>
	/// Id type configuration provider for community edition.
	/// Allow to reuse predefined set of id types. </summary>
	/// <seealso cref= IdType </seealso>
	/// <seealso cref= IdTypeConfiguration </seealso>
	public class CommunityIdTypeConfigurationProvider : IdTypeConfigurationProvider
	{

		 private static readonly ISet<IdType> _typesToAllowReuse = Collections.unmodifiableSet( EnumSet.of( IdType.PROPERTY, IdType.STRING_BLOCK, IdType.ARRAY_BLOCK, IdType.NODE_LABELS ) );

		 private readonly IDictionary<IdType, IdTypeConfiguration> _typeConfigurations = new Dictionary<IdType, IdTypeConfiguration>( typeof( IdType ) );

		 public override IdTypeConfiguration GetIdTypeConfiguration( IdType idType )
		 {
			  return _typeConfigurations.computeIfAbsent( idType, this.createIdTypeConfiguration );
		 }

		 private IdTypeConfiguration CreateIdTypeConfiguration( IdType idType )
		 {
			  return new IdTypeConfiguration( TypesToReuse.Contains( idType ) );
		 }

		 protected internal virtual ISet<IdType> TypesToReuse
		 {
			 get
			 {
				  return _typesToAllowReuse;
			 }
		 }
	}

}