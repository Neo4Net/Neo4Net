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
namespace Neo4Net.Jmx.impl
{

	using Service = Neo4Net.Helpers.Service;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) @Deprecated public final class PrimitivesBean extends ManagementBeanProvider
	[Obsolete]
	public sealed class PrimitivesBean : ManagementBeanProvider
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public PrimitivesBean()
		 public PrimitivesBean() : base(typeof(Primitives))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4NetMBean createMBean(ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
		 {
			  return new PrimitivesImpl( management );
		 }

		 private class PrimitivesImpl : Neo4NetMBean, Primitives
		 {
			  internal readonly IdGeneratorFactory IdGeneratorFactory;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PrimitivesImpl(ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal PrimitivesImpl( ManagementData management ) : base( management )
			  {
					this.IdGeneratorFactory = management.ResolveDependency( typeof( IdGeneratorFactory ) );
			  }

			  public virtual long NumberOfNodeIdsInUse
			  {
				  get
				  {
						return IdGeneratorFactory.get( IdType.NODE ).NumberOfIdsInUse;
				  }
			  }

			  public virtual long NumberOfRelationshipIdsInUse
			  {
				  get
				  {
						return IdGeneratorFactory.get( IdType.RELATIONSHIP ).NumberOfIdsInUse;
				  }
			  }

			  public virtual long NumberOfPropertyIdsInUse
			  {
				  get
				  {
						return IdGeneratorFactory.get( IdType.PROPERTY ).NumberOfIdsInUse;
				  }
			  }

			  public virtual long NumberOfRelationshipTypeIdsInUse
			  {
				  get
				  {
						return IdGeneratorFactory.get( IdType.RELATIONSHIP_TYPE_TOKEN ).NumberOfIdsInUse;
				  }
			  }
		 }
	}

}