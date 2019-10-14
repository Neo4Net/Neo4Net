using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.cluster.client
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;


	public class Cluster
	{
		 private readonly string _name;
		 private readonly IList<Member> _members = new List<Member>();

		 /// <summary>
		 /// Creates a cluster with a random name.
		 /// The name is used in deciding which directory the store files are placed in.
		 /// </summary>
		 public Cluster() : this(RandomStringUtils.randomAlphanumeric(8))
		 {
		 }

		 /// <summary>
		 /// Creates a cluster with a specified name.
		 /// The name is used in deciding which directory the store files are placed in.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public Cluster(@Nonnull String name)
		 public Cluster( string name )
		 {
			  requireNonNull( name );
			  this._name = name;
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public virtual IList<Member> Members
		 {
			 get
			 {
				  return _members;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  Cluster cluster = ( Cluster ) o;

//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: return name.equals(cluster.name) && members.equals(cluster.members);
			  return _name.Equals( cluster._name ) && _members.SequenceEqual( cluster._members );

		 }

		 public override int GetHashCode()
		 {
			  int result = _name.GetHashCode();
			  result = 31 * result + _members.GetHashCode();
			  return result;
		 }

		 /// <summary>
		 /// Represents a member tag in the discovery XML file.
		 /// </summary>
		 public class Member
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string HostConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool FullHaMemberConflict;

			  public Member( int port, bool fullHaMember ) : this( Localhost() + ":" + port, fullHaMember )
			  {
			  }

			  public Member( string host, bool fullHaMember )
			  {
					this.HostConflict = host;
					this.FullHaMemberConflict = fullHaMember;
			  }

			  public virtual bool FullHaMember
			  {
				  get
				  {
						return FullHaMemberConflict;
				  }
			  }

			  internal static string Localhost()
			  {
					return "localhost";
			  }

			  public virtual string Host
			  {
				  get
				  {
						return HostConflict;
				  }
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}

					Member member = ( Member ) o;

					return HostConflict.Equals( member.HostConflict );

			  }

			  public override int GetHashCode()
			  {
					return HostConflict.GetHashCode();
			  }
		 }
	}

}