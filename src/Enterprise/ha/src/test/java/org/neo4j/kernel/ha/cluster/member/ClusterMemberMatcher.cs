using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.ha.cluster.member
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;


	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using ClusterMemberInfo = Neo4Net.management.ClusterMemberInfo;

	public class ClusterMemberMatcher : BaseMatcher<IEnumerable<ClusterMemberInfo>>
	{
		 private bool _exactMatch;
		 private ClusterMemberMatch[] _expectedMembers;

		 public ClusterMemberMatcher( bool exactMatch, ClusterMemberMatch[] expected )
		 {
			  this._exactMatch = exactMatch;
			  this._expectedMembers = expected;
		 }

		 public override void DescribeTo( Description description )
		 {
			  description.appendText( Arrays.ToString( _expectedMembers ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<ClusterMember> sameMemberAs(final ClusterMember clusterMember)
		 public static Matcher<ClusterMember> SameMemberAs( ClusterMember clusterMember )
		 {
			  return new BaseMatcherAnonymousInnerClass( clusterMember );
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<ClusterMember>
		 {
			 private Neo4Net.Kernel.ha.cluster.member.ClusterMember _clusterMember;

			 public BaseMatcherAnonymousInnerClass( Neo4Net.Kernel.ha.cluster.member.ClusterMember clusterMember )
			 {
				 this._clusterMember = clusterMember;
			 }

			 public override bool matches( object instance )
			 {
				  if ( instance is ClusterMember )
				  {

						ClusterMember member = typeof( ClusterMember ).cast( instance );
						if ( !member.InstanceId.Equals( _clusterMember.InstanceId ) )
						{
							 return false;
						}

						if ( member.Alive != _clusterMember.Alive )
						{
							 return false;
						}

						HashSet<URI> memberUris = new HashSet<URI>( Iterables.asList( member.RoleURIs ) );
						HashSet<URI> clusterMemberUris = new HashSet<URI>( Iterables.asList( _clusterMember.RoleURIs ) );
						return memberUris.Equals( clusterMemberUris );
				  }
				  else
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "member should match " ).appendValue( _clusterMember );
			 }
		 }

		 public override bool Matches( object item )
		 {
			  if ( item is System.Collections.IEnumerable )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Iterable<org.neo4j.management.ClusterMemberInfo> other = (Iterable<org.neo4j.management.ClusterMemberInfo>) item;
					IEnumerable<ClusterMemberInfo> other = ( IEnumerable<ClusterMemberInfo> ) item;
					int foundCount = 0;
					foreach ( ClusterMemberMatch expectedMember in _expectedMembers )
					{
						 bool found = false;
						 foreach ( ClusterMemberInfo member in other )
						 {
							  if ( expectedMember.Match( member ) )
							  {
									found = true;
									foundCount++;
									break;
							  }
						 }
						 if ( !found )
						 {
							  return false;
						 }
					}

					if ( _exactMatch && foundCount != _expectedMembers.Length )
					{
						 return false;
					}
			  }
			  else
			  {
					return false;
			  }
			  return true;
		 }

		 public static ClusterMemberMatch Member( URI member )
		 {
			  return new ClusterMemberMatch( member );
		 }

		 public static ClusterMemberMatcher ContainsMembers( params ClusterMemberMatch[] expected )
		 {
			  return new ClusterMemberMatcher( false, expected );
		 }

		 public static ClusterMemberMatcher ContainsOnlyMembers( params ClusterMemberMatch[] expected )
		 {
			  return new ClusterMemberMatcher( true, expected );
		 }

		 public class ClusterMemberMatch
		 {
			  internal URI Member;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool? AvailableConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool? AliveConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string HaRoleConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal ISet<string> UrisConflict;

			  internal ClusterMemberMatch( URI member )
			  {
					this.Member = member;
			  }

			  public virtual ClusterMemberMatch Available( bool available )
			  {
					this.AvailableConflict = available;
					return this;
			  }

			  public virtual ClusterMemberMatch Alive( bool alive )
			  {
					this.AliveConflict = alive;
					return this;
			  }

			  internal virtual bool Match( ClusterMemberInfo toMatch )
			  {
					if ( !Member.ToString().Equals(toMatch.InstanceId) )
					{
						 return false;
					}
					if ( AvailableConflict != null && toMatch.Available != AvailableConflict.Value )
					{
						 return false;
					}
					if ( AliveConflict != null && toMatch.Alive != AliveConflict.Value )
					{
						 return false;
					}
					if ( !string.ReferenceEquals( HaRoleConflict, null ) && !HaRoleConflict.Equals( toMatch.HaRole ) )
					{
						 return false;
					}
					return !( UrisConflict != null && !UrisConflict.SetEquals( new HashSet<>( asList( toMatch.Uris ) ) ) );
			  }

			  public override string ToString()
			  {
					StringBuilder builder = new StringBuilder( "Member[" + Member );
					if ( AvailableConflict != null )
					{
						 builder.Append( ", available:" ).Append( AvailableConflict );
					}
					if ( AliveConflict != null )
					{
						 builder.Append( ", alive:" ).Append( AliveConflict );
					}
					if ( !string.ReferenceEquals( HaRoleConflict, null ) )
					{
						 builder.Append( ", haRole:" ).Append( HaRoleConflict );
					}
					if ( UrisConflict != null )
					{
						 builder.Append( ", uris:" ).Append( UrisConflict );
					}
					return builder.Append( "]" ).ToString();
			  }

			  public virtual ClusterMemberMatch HaRole( string role )
			  {
					this.HaRoleConflict = role;
					return this;
			  }

			  public virtual ClusterMemberMatch Uris( params URI[] uris )
			  {
					this.UrisConflict = new HashSet<string>();
					foreach ( URI uri in uris )
					{
						 this.UrisConflict.Add( uri.ToString() );
					}
					return this;
			  }
		 }
	}

}