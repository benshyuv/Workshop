using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using DomainLayer.Exceptions;
using CustomLogger;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Users;

[assembly: InternalsVisibleTo("DomainLayerTests")]
namespace DomainLayer.Stores.Certifications
{
    public enum Permission { INVENTORY = 1, POLICY = 2, APPOINT_OWNER = 4, REMOVE_OWNER = 8, 
                            APPOINT_MANAGER = 16, EDIT_PERMISSIONS = 32, REMOVE_MANAGER = 64, 
                            CLOSE_STORE = 128, REQUESTS = 256, HISTORY = 512 }
    
    public class Certification
    {
        [Key, ForeignKey("User")]
        public Guid UserID { get; set; }
        public virtual RegisteredUser User { get; set; }

        [Key, ForeignKey("Store")]
        public Guid StoreID { get; set; }
        public virtual Store Store { get; set; }

        [ForeignKey("GrantorUser")]
        public Guid? Grantor { get; set; }
        public virtual RegisteredUser GrantorUser { get; set; }

        [ForeignKey("Grantor, StoreID")]
        public virtual Certification GrantorCert { get; set; }

        [InverseProperty("GrantorCert")]
        public virtual ICollection<Certification> GrantedByMe { get; set; }
        public bool Owner { get; set; }

        public Permission permissions { get; set; }
        internal ISet<Permission> Permissions
        {
            get
            {
                if (Owner)
                {
                    return new HashSet<Permission>((Permission[])Enum.GetValues(typeof(Permission)));
                }
                else
                {
                    HashSet<Permission> result = new HashSet<Permission>();
                    foreach (Permission perm in Enum.GetValues(typeof(Permission)))
                    {
                        if (permissions.HasFlag(perm))
                        {
                            result.Add(perm);
                        }
                    }
                    return result;
                }
            }
            set
            {
                permissions = 0;
                if (!(value is null))
                {
                    foreach (Permission perm in value)
                    {
                        permissions |= perm;
                    }
                }
                
            }
        }

        internal Certification(Guid userID, Guid storeID, Guid? grantor, bool owner, ISet<Permission> permissions)
        {
            UserID = userID;
            StoreID = storeID;
            Grantor = grantor;
            Owner = owner;
            Permissions = permissions;
            GrantedByMe = new HashSet<Certification>();
        }

        public Certification()
        {
        }

        internal ISet<Guid> GetUsersIDsGrantedByMe()
        {
            HashSet<Guid> ret = new HashSet<Guid>();
            foreach(Certification cert in GrantedByMe)
            {
                ret.Add(cert.UserID);
            }
            return ret;
        }

        internal void RemovePermission(Permission toRemove)
        {
            if (IsOwner())
            {
                Logger.writeEvent("Certification: RemovePermission| User: is an owner");
                throw new PermissionException("Cannot change owner permissions for owner");
            }
            if (!permissions.HasFlag(toRemove))
            {
                Logger.writeEvent(string.Format("Certification: RemovePermission| Can't remove non-existing permission \'{0}\'",
                                                                toRemove));
                throw new PermissionException(string.Format("Can't remove non exsiting permission \'{0}\'", toRemove));
            }
            permissions ^= toRemove;
            Logger.writeEvent(string.Format("Certification: RemovePermission| removed permission \'{0}\'", toRemove));
        }

        internal void AddPermission(Permission toAdd)
        {
            if (IsOwner())
            {
                Logger.writeEvent("Certification: AddPermission| User: is an owner");
                throw new PermissionException("Cannot change owner permissions for owner");
            }
            if (permissions.HasFlag(toAdd))
            {
                Logger.writeEvent(string.Format("Certification: AddPermission| Can't add existing permission \'{0}\'", toAdd));
                throw new PermissionException(string.Format("Can't add exsiting permission \'{0}\'", toAdd));
            }
            permissions |= toAdd;
            Logger.writeEvent(string.Format("Certification: AddPermission| added permission \'{0}\'", toAdd));
        }

        internal bool IsOwner() => Owner;

        internal bool IsFirstOwner() => IsGrantor(Guid.Empty);

        internal bool IsGrantor(Guid username) => username == Grantor;

        internal bool IsPermitted(Permission permission) => IsOwner() || permissions.HasFlag(permission);

        public virtual bool ShouldSerializeStore()
        {
            return false;
        }

        public virtual bool ShouldSerializeGrantorCert()
        {
            return false;
        }

        internal void RemoveGrantedByMe(Certification cert)
        {
            GrantedByMe.Remove(cert);
        }

        internal void AddGrantedByMe(Certification cert)
        {
            GrantedByMe.Add(cert);
        }
    }
}
